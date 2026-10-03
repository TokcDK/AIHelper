using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModUpdater.Core.Infrastructure.Install;
using NLog;

namespace AIHelper.Manage.Update.Installers
{
    /// <summary>
    /// Keeps the meta.ini of a mod across an install.
    /// <para>
    /// An install overwrites whatever the archive contains, and a mod archive is
    /// not expected to carry a meta.ini — but if one ever does, it would replace
    /// the file holding the update record and the Mod Organizer category of the mod,
    /// and the mod would silently stop being updatable. The content is therefore
    /// kept aside and put back when the install replaced it.
    /// </para>
    /// <para>
    /// A decorator rather than a second archive installer: which archives get
    /// extracted is not this class's business, so it asks the installer it wraps and
    /// only adds the one thing it knows about.
    /// </para>
    /// </summary>
    internal sealed class MetaIniPreservingInstaller : IInstaller
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>File Mod Organizer keeps its per mod data in.</summary>
        private const string MetaIniFileName = "meta.ini";

        private readonly IInstaller _inner;

        internal MetaIniPreservingInstaller(IInstaller inner)
        {
            _inner = inner;
        }

        /// <summary>
        /// Decided by the installer this one wraps. A decorator adds a preservation step
        /// around the inner installer and neither downloads the update nor rolls it back
        /// itself, so the engine has to treat the pair exactly as it treats the inner one.
        /// </summary>
        public bool IsSelfContained
        {
            get { return _inner != null && _inner.IsSelfContained; }
        }

        public bool CanInstall(InstallContext context)
        {
            return _inner != null && _inner.CanInstall(context);
        }

        public async Task<InstallResult> InstallAsync(InstallContext context, CancellationToken ct)
        {
            var metaIniPath = Path.Combine(context.TargetInstallPath ?? string.Empty, MetaIniFileName);
            var preservedContent = TryRead(metaIniPath);

            var result = await _inner.InstallAsync(context, ct).ConfigureAwait(false);

            if (result.Success && preservedContent != null)
            {
                RestoreIfReplaced(metaIniPath, preservedContent);
            }

            return result;
        }

        private static string TryRead(string metaIniPath)
        {
            try
            {
                return File.Exists(metaIniPath) ? File.ReadAllText(metaIniPath) : null;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to read '" + metaIniPath + "' before installing.");
                return null;
            }
        }

        /// <summary>
        /// Puts the kept content back when the install changed the file. An install
        /// that left it alone is not touched at all, so the file keeps its timestamp.
        /// </summary>
        private static void RestoreIfReplaced(string metaIniPath, string preservedContent)
        {
            try
            {
                if (File.Exists(metaIniPath)
                    && string.Equals(File.ReadAllText(metaIniPath), preservedContent, StringComparison.Ordinal))
                {
                    return;
                }

                UpdateIo.EnsureParentDirectory(metaIniPath);
                File.WriteAllText(metaIniPath, preservedContent);

                _log.Info("Update: restored '" + metaIniPath + "' after the install replaced it.");
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Update: failed to restore '" + metaIniPath + "' after installing.");
            }
        }
    }
}
