using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using AIHelper.Manage.Update.Backup;
using AIHelper.Manage.Update.Installers;
using AIHelper.Manage.Update.Versioning;
using ModUpdater.Core.Application;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure;
using ModUpdater.Core.Infrastructure.Install;
using ModUpdater.Core.Infrastructure.Sources;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Assembles the update engine for one pass.
    /// <para>
    /// This is the composition root of the update feature: the single place that
    /// knows which concrete source, version store, installers and backup policy the
    /// application uses. Everything downstream works against the engine's interfaces,
    /// so swapping one of them is a change here and nowhere else.
    /// </para>
    /// <para>
    /// The engine's own defaults are deliberately not used for two of them. The
    /// version store is meta.ini rather than the engine's JSON manifest, because a
    /// stray file in a mod folder shows up as a Mod Organizer conflict. The backup is
    /// a folder copy rather than a zip, because packing a mod folder follows the
    /// symlinks this application creates and would back up the game instead.
    /// </para>
    /// </summary>
    internal sealed class UpdateEngineFactory : IDisposable
    {
        /// <summary>
        /// GitHub rejects a request that carries no User-Agent, so one is sent even
        /// though nothing on the other side reads it.
        /// </summary>
        private const string UserAgent = "AIHelper";

        /// <summary>
        /// A release download can be large and a slow connection is not an error, so
        /// the default hundred seconds is far too short.
        /// </summary>
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(10);

        private readonly HttpClient _httpClient;

        internal UpdateEngineFactory()
        {
            EnsureModernTls();

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            _httpClient.Timeout = RequestTimeout;

            // The HTML source is used rather than the API one: it is what this
            // application has always read, and it does not run into the sixty
            // requests per hour the API allows an unauthenticated caller.
            GithubSource = new GithubScrapeSource(new HttpDownloadClient(_httpClient));
        }

        /// <summary>
        /// Source the update pass reads release information from.
        /// </summary>
        internal IUpdateSource GithubSource { get; }

        /// <summary>
        /// Builds the engine that will process the given targets.
        /// </summary>
        internal IUpdateManager CreateManager(IReadOnlyList<UpdateTarget> targets)
        {
            var modOrganizerInstallPath = ManageSettings.AppModOrganizerDirPath;

            var versionProvider = new MetaIniVersionProvider(modOrganizerInstallPath);
            var checkService = new UpdateCheckService(versionProvider, new UpdateToolsVersionComparer());

            // Order matters: the resolver takes the first installer that accepts the
            // asset. The archive installer is wrapped so that a mod keeps its meta.ini,
            // and the Mod Organizer installer claims the one target that runs an
            // installer instead of extracting.
            var installerResolver = new InstallerResolver(new IInstaller[]
            {
                new MetaIniPreservingInstaller(new ArchiveInstaller()),
                new SingleFileInstaller(),
                new ModOrganizerInstaller(new RunInstallerInstaller(), modOrganizerInstallPath)
            });

            var backupService = new ModFolderBackupService(modOrganizerInstallPath);

            return new UpdateManager(checkService, installerResolver, backupService, versionProvider, targets);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        /// <summary>
        /// GitHub serves its pages over TLS 1.2 and newer only, while .NET Framework
        /// otherwise negotiates whatever the operating system offers. The protocol is
        /// pinned, as the hand written source did before it was replaced.
        /// </summary>
        private static void EnsureModernTls()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }
}
