using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Remembers the card files which were already filed by their content, so the
    /// installers which work by file name can skip them.
    /// </summary>
    /// <remarks>
    /// The installers of one run are created and called in the order of their
    /// <see cref="Types.ModInstallerBase.Order"/>, and the content based installer
    /// runs first. Registering every card it moved keeps a later installer from
    /// processing the same card a second time, which matters when a card could not
    /// be moved and is still lying in the install folder.
    /// </remarks>
    internal static class CardTargetRegistry
    {
        private static readonly HashSet<string> ClaimedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Marks a card as filed, so other installers leave it alone.
        /// </summary>
        internal static void Register(string path)
        {
            var key = Normalize(path);
            if (key == null) return;

            lock (SyncRoot)
            {
                ClaimedPaths.Add(key);
            }
        }

        /// <summary>
        /// Reports whether the passed file was already filed by its content.
        /// </summary>
        internal static bool IsClaimed(string path)
        {
            var key = Normalize(path);
            if (key == null) return false;

            lock (SyncRoot)
            {
                return ClaimedPaths.Contains(key);
            }
        }

        /// <summary>
        /// Forgets every filed card. Must be called at the start of an install run,
        /// otherwise a card placed at a path used by an earlier run would be skipped.
        /// </summary>
        internal static void Clear()
        {
            lock (SyncRoot)
            {
                ClaimedPaths.Clear();
            }
        }

        private static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            try
            {
                return Path.GetFullPath(path);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (PathTooLongException)
            {
                return null;
            }
        }
    }
}
