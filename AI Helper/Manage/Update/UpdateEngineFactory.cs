using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using AIHelper.Manage.Update.Backup;
using AIHelper.Manage.Update.Installers;
using AIHelper.Manage.Update.SelfUpdate;
using AIHelper.Manage.Update.Versioning;
using ModUpdater.Core.Application;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure;
using ModUpdater.Core.Infrastructure.Install;
using ModUpdater.Core.Infrastructure.Sources;
using NLog;

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
    /// The engine's own defaults are deliberately not used for four of them. The
    /// version store is meta.ini rather than the engine's JSON manifest, because a
    /// stray file in a mod folder shows up as a Mod Organizer conflict; the skip
    /// list is kept in the same place for the same reason. The backup is a folder
    /// copy rather than a zip, because packing a mod folder follows the symlinks
    /// this application creates and would back up the game instead. And the version
    /// store, the installers and the backup all have to know which folders are not
    /// mods, which is why they are told rather than left to work it out.
    /// </para>
    /// </summary>
    internal sealed class UpdateEngineFactory : IDisposable
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

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
        private readonly IReadOnlyList<string> _nonModFolderPaths;

        /// <param name="nonModFolderPaths">
        /// Folders of this pass that are not mods: the Mod Organizer installation and the
        /// application itself. They are told apart because they have no meta.ini of their own
        /// and because a copy of one is not a version anybody wants to go back to.
        /// <para>
        /// Optional, because a factory is also built for its source alone - the info editor
        /// downloads a release with it and never creates a manager - and a list that is only
        /// read by <see cref="CreateManager"/> has no meaning for a caller that does not call it.
        /// </para>
        /// </param>
        internal UpdateEngineFactory(IReadOnlyCollection<string> nonModFolderPaths = null)
        {
            _nonModFolderPaths = nonModFolderPaths == null
                ? new List<string>()
                : new List<string>(nonModFolderPaths);

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
            var appUpdatePath = SelfUpdatePaths.PendingUpdateDirectory;

            // A mod keeps its version in its own meta.ini; the two targets that are not mods
            // have no meta.ini, and the application answers from its program file instead. Its
            // version is also declared with the record by the pass, which is what the update
            // window shows before the first check - see ApplicationVersionProvider for why the
            // two are both there and cannot disagree.
            var versionProvider = new ApplicationVersionProvider(
                new MetaIniVersionProvider(_nonModFolderPaths), appUpdatePath);

            // The skip list is stored where this application already keeps per game
            // update data (RES\update\<game>) and inside each mod's meta.ini, so the
            // engine's own JSON store is replaced the same way the version provider is.
            var skippedVersionStore = new SkippedVersionStore(ManageSettings.CurrentGameModsUpdateDir);

            // The engine has no logging of its own, so without this delegate a source it
            // could not reach would look exactly like a successful check that found nothing.
            var checkService = new UpdateCheckService(
                versionProvider, new UpdateToolsVersionComparer(), LogEngineMessage);

            // Order matters: the resolver takes the first installer that accepts the
            // asset. The application claims its own target because its update has to say
            // that it takes effect on the next start; the archive installer is wrapped so
            // that a mod keeps its meta.ini; and the Mod Organizer installer claims the one
            // target that runs an installer instead of extracting.
            var installerResolver = new InstallerResolver(new IInstaller[]
            {
                new AppUpdateInstaller(new ArchiveInstaller(), appUpdatePath),
                new MetaIniPreservingInstaller(new ArchiveInstaller()),
                new SingleFileInstaller(),
                new ModOrganizerInstaller(new RunInstallerInstaller(), modOrganizerInstallPath)
            });

            var backupService = new ModFolderBackupService(_nonModFolderPaths);

            return new UpdateManager(
                checkService, installerResolver, backupService, versionProvider, targets,
                skippedVersionStore);
        }

        /// <summary>
        /// Routes a message the engine cannot show itself (a skipped source, a skipped
        /// version) into this application's log.
        /// </summary>
        private static void LogEngineMessage(string message)
        {
            _log.Info(message);
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
