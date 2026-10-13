using System;
using System.IO;

namespace AIHelperUpdater
{
    /// <summary>
    /// Moves files and whole folder trees, overwriting what is already there.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two details are the reason this is a type of its own rather than a few lines at the call
    /// site.
    /// </para>
    /// <para>
    /// The first is that .NET Framework has no overwriting move: File.Move refuses an existing
    /// destination, while the whole point of an update is to write over what is there. The
    /// existing file is therefore removed first - and that is exactly why the backup is taken
    /// before the payload is applied, so there is always something to go back to.
    /// </para>
    /// <para>
    /// The second is that a tree is walked to a list before anything is moved out of it.
    /// Enumerating a folder while moving its entries is a way to visit an entry twice or to miss
    /// one, and the entries here are being moved out from under the walk.
    /// </para>
    /// <para>
    /// A folder that is not moved in full - because a reserved folder is inside it - is left
    /// behind with what was not moved, rather than deleted: the caller's skip rule names things
    /// that have to survive.
    /// </para>
    /// </remarks>
    internal static class FileTreeMover
    {
        /// <summary>
        /// Moves a file over an existing one, creating the destination folder when it is missing.
        /// </summary>
        /// <param name="sourceFilePath">File to move.</param>
        /// <param name="destinationFilePath">Where it has to end up, existing or not.</param>
        internal static void MoveFileOverwrite(string sourceFilePath, string destinationFilePath)
        {
            var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrEmpty(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

            if (File.Exists(destinationFilePath)) File.Delete(destinationFilePath);

            File.Move(sourceFilePath, destinationFilePath);
        }

        /// <summary>
        /// Moves a folder tree, leaving behind whatever the skip rule names.
        /// </summary>
        /// <param name="sourceDirectoryPath">Folder to move out of.</param>
        /// <param name="destinationDirectoryPath">Folder to move into; created when missing.</param>
        /// <param name="isSkipped">
        /// Asked about (source, destination) of every entry, and about a folder before it is
        /// entered. True means the entry is left where it is. Both paths are given because which
        /// one decides differs by direction: taking a backup is about what may leave the
        /// application, applying one is about what may be written to.
        /// </param>
        /// <param name="report">Told about every entry that was left behind, or null.</param>
        /// <returns>Number of files moved.</returns>
        internal static int MoveDirectory(
            string sourceDirectoryPath,
            string destinationDirectoryPath,
            Func<string, string, bool> isSkipped,
            Action<string> report)
        {
            Directory.CreateDirectory(destinationDirectoryPath);

            var movedFileCount = 0;

            // To a list first: entries are moved out of this folder as the loop runs, and a
            // folder that is being changed under the walk is not a folder that can be walked.
            var entries = Directory.GetFileSystemEntries(sourceDirectoryPath);

            foreach (var entry in entries)
            {
                var destination = Path.Combine(destinationDirectoryPath, Path.GetFileName(entry));

                if (isSkipped != null && isSkipped(entry, destination))
                {
                    if (report != null) report("left in place: " + entry);
                    continue;
                }

                if (Directory.Exists(entry))
                {
                    movedFileCount += MoveDirectory(entry, destination, isSkipped, report);
                }
                else
                {
                    MoveFileOverwrite(entry, destination);
                    movedFileCount++;
                }
            }

            // Only a folder everything was moved out of can go; one that still holds something
            // that had to be left in place is not empty and stays.
            TryRemoveEmptyDirectory(sourceDirectoryPath);

            return movedFileCount;
        }

        /// <summary>
        /// Removes a folder if it exists and is empty. Never throws.
        /// </summary>
        /// <param name="directoryPath">Folder to remove.</param>
        /// <returns>True when the folder is gone.</returns>
        internal static bool TryRemoveEmptyDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return true;

            try
            {
                if (!Directory.Exists(directoryPath)) return true;

                Directory.Delete(directoryPath, false);
                return true;
            }
            catch (Exception)
            {
                // either it is not empty or it is not ours to remove; both are fine here
                return !Directory.Exists(directoryPath);
            }
        }

        /// <summary>
        /// Removes a folder and everything in it. Never throws.
        /// </summary>
        /// <param name="directoryPath">Folder to remove.</param>
        /// <returns>True when the folder is gone.</returns>
        internal static bool TryRemoveDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return true;

            try
            {
                if (!Directory.Exists(directoryPath)) return true;

                Directory.Delete(directoryPath, true);
                return true;
            }
            catch (Exception)
            {
                return !Directory.Exists(directoryPath);
            }
        }
    }
}
