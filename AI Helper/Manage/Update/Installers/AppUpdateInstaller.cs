using System;
using System.Threading;
using System.Threading.Tasks;
using AIHelper.Manage;
using ModUpdater.Core.Infrastructure.Install;

namespace AIHelper.Manage.Update.Installers
{
    /// <summary>
    /// Installs the application's own update, which is unpacked into a folder of its own
    /// instead of over the program that is running.
    /// </summary>
    /// <remarks>
    /// <para>
    /// What the archive installer does is right, and the extraction kind asks for the one thing
    /// this update needs on top of it: the payload folder is emptied first, so that a file of an
    /// earlier download cannot survive into the application. This decorator adds what that
    /// installer cannot know - that the install is not the end of the update.
    /// </para>
    /// <para>
    /// The folder the release is unpacked into is read by aihelper-updater on the next start, and
    /// nothing on screen changes until then. A user who is told "updated to 26.10.13.6" and then
    /// finds the application on the old version has been told something untrue, so the sentence
    /// that says when it takes effect is part of the result.
    /// </para>
    /// <para>
    /// A decorator rather than a second archive installer: which archives get extracted is not
    /// this class's business, so it asks the installer it wraps and only adds the message.
    /// </para>
    /// </remarks>
    internal sealed class AppUpdateInstaller : IInstaller
    {
        private readonly IInstaller _inner;
        private readonly string _updateFolderPath;

        /// <param name="inner">Installer that unpacks the release.</param>
        /// <param name="updateFolderPath">
        /// Folder the application's own update is unpacked into, which is how the one target
        /// this class answers for is recognised.
        /// </param>
        internal AppUpdateInstaller(IInstaller inner, string updateFolderPath)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));

            _inner = inner;
            _updateFolderPath = updateFolderPath ?? string.Empty;
        }

        /// <summary>
        /// Decided by the installer this one wraps. A decorator adds a message to the result
        /// and neither downloads the update nor rolls it back itself, so the engine has to
        /// treat the pair exactly as it treats the inner one.
        /// </summary>
        public bool IsSelfContained
        {
            get { return _inner.IsSelfContained; }
        }

        public bool CanInstall(InstallContext context)
        {
            return IsApplication(context) && _inner.CanInstall(context);
        }

        public async Task<InstallResult> InstallAsync(InstallContext context, CancellationToken ct)
        {
            var result = await _inner.InstallAsync(context, ct).ConfigureAwait(false);

            if (result.Success)
            {
                result.Message = T._("The update is applied the next time AI Helper starts.");
            }

            return result;
        }

        /// <summary>
        /// True for the application's own target. The engine hands the installer the target's
        /// install path and nothing else that identifies it.
        /// </summary>
        private bool IsApplication(InstallContext context)
        {
            if (context == null || _updateFolderPath.Length == 0) return false;

            return string.Equals(
                context.TargetInstallPath, _updateFolderPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
