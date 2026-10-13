using System;
using System.Threading;
using System.Threading.Tasks;
using AIHelper.Manage.Update.SelfUpdate;
using ModUpdater.Core.Domain;
using ModUpdater.Core.Infrastructure.Versioning;

namespace AIHelper.Manage.Update.Versioning
{
    /// <summary>
    /// Answers the installed version of AI Helper, which is not written anywhere of its own:
    /// it is what the build stamped into the program.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The application is asked about in the same way as a mod, and answered differently,
    /// because there is no file of its own to keep a version in. What is read is the newer of the
    /// two program files that can carry it - the waiting update and the running program - so
    /// that a session which has already downloaded a release does not offer the same release
    /// again. See <see cref="ApplicationVersion.ReadInstalled"/>.
    /// </para>
    /// <para>
    /// The same version is also declared with the target's record by the pass itself
    /// (<c>Updater.DeclaredVersionOf</c>), and the two are not a duplication to be collapsed:
    /// the record is read once, when the pass is built, and is what the update window shows
    /// before the first check and after a check that found nothing; this store is asked again on
    /// every check, which is the only way a release unpacked into the payload folder *during*
    /// the session stops being offered. The engine takes the newer of the two answers, so they
    /// can only ever agree.
    /// </para>
    /// <para>
    /// The version is not written back after an install. There is nowhere to write it: the
    /// program file carries it, and the payload folder is emptied and refilled by the next
    /// release. Everything that has to know which version is coming reads it from the file, so
    /// a record of it kept somewhere else could only ever disagree with it.
    /// </para>
    /// </remarks>
    internal sealed class ApplicationVersionProvider : IInstalledVersionProvider
    {
        private readonly IInstalledVersionProvider _otherTargets;
        private readonly string _updateFolderPath;

        /// <param name="otherTargets">
        /// Store the targets that are not the application are read from and written to - the
        /// meta.ini of a mod.
        /// </param>
        /// <param name="updateFolderPath">
        /// Folder the application's own update is unpacked into, which is how the one target
        /// this class answers for is recognised.
        /// </param>
        internal ApplicationVersionProvider(IInstalledVersionProvider otherTargets, string updateFolderPath)
        {
            if (otherTargets == null) throw new ArgumentNullException(nameof(otherTargets));

            _otherTargets = otherTargets;
            _updateFolderPath = updateFolderPath ?? string.Empty;
        }

        public Task<string> GetCurrentVersionAsync(UpdateTarget target, AssetSpec asset, CancellationToken ct)
        {
            if (!IsApplication(target))
            {
                return _otherTargets.GetCurrentVersionAsync(target, asset, ct);
            }

            return Task.FromResult(ApplicationVersion.ReadInstalled());
        }

        /// <summary>
        /// Does nothing for the application: the version is in the program file and not in a
        /// record this store could write.
        /// </summary>
        public Task SetCurrentVersionAsync(
            UpdateTarget target, AssetSpec asset, VersionInfo version, CancellationToken ct)
        {
            if (IsApplication(target)) return Task.CompletedTask;

            return _otherTargets.SetCurrentVersionAsync(target, asset, version, ct);
        }

        /// <summary>
        /// True for the application's own target. The engine hands this store a target and
        /// nothing else that identifies it, and the install path is the one thing that tells
        /// this target apart from a mod.
        /// </summary>
        private bool IsApplication(UpdateTarget target)
        {
            if (target == null || _updateFolderPath.Length == 0) return false;

            return string.Equals(
                target.InstallPath, _updateFolderPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
