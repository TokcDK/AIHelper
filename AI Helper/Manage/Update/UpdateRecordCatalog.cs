using System;
using System.Collections.Generic;
using AIHelper.Manage.Update.Providers;
using NLog;

namespace AIHelper.Manage.Update
{
    /// <summary>
    /// Collects the updatable folders of every provider into one list.
    /// <para>
    /// The providers overlap on purpose — a mod may be listed in updateinfo.txt and
    /// carry a record in meta.ini at the same time — so the first provider to name a
    /// folder wins and the later ones are ignored for it. Which provider wins is the
    /// order they were added in, which is why the caller adds them explicitly.
    /// </para>
    /// </summary>
    internal sealed class UpdateRecordCatalog
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly List<IUpdateRecordProvider> _providers = new List<IUpdateRecordProvider>();

        /// <summary>Adds a provider. Null is ignored so a caller can add conditionally.</summary>
        internal void Add(IUpdateRecordProvider provider)
        {
            if (provider == null) return;

            _providers.Add(provider);
        }

        /// <summary>
        /// Asks every provider for its folders and returns the ones that are usable.
        /// </summary>
        /// <returns>Folders to update, each named exactly once.</returns>
        internal IReadOnlyList<UpdatableFolder> Collect()
        {
            var result = new List<UpdatableFolder>();
            var claimedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in _providers)
            {
                foreach (var folder in provider.GetUpdatableFolders())
                {
                    if (folder == null || folder.Record == null) continue;

                    if (!folder.Record.IsValid)
                    {
                        _log.Warn("Update: skipped '" + folder.FolderPath
                                  + "'. The update record needs an owner, a repository and a file name prefix.");
                        continue;
                    }

                    if (!claimedPaths.Add(folder.FolderPath)) continue;

                    result.Add(folder);
                }
            }

            return result;
        }
    }
}
