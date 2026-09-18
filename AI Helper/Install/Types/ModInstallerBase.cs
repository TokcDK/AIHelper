using AIHelper.Forms.Other;
using NLog;
using System;
using System.IO;

namespace AIHelper.Install.Types
{
    /// <summary>
    /// Base class for installers which are processing content of the install folder
    /// </summary>
    abstract class ModInstallerBase
    {
        protected static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// order of installer
        /// </summary>
        public virtual int Order => 1000;

        /// <summary>
        /// install from default 2MO folder
        /// </summary>
        public abstract bool Install();

        /// <summary>
        /// install from specific folder
        /// </summary>
        /// <param name="path"></param>
        public abstract bool InstallFrom(string path);

        /// <summary>
        /// mask of installer's objects
        /// </summary>
        public abstract string[] Masks { get; }

        /// <summary>
        /// Enumerates content of <paramref name="directoryInfo"/> by every mask from <see cref="Masks"/>
        /// and passes each found entry to <paramref name="processEntry"/>.
        /// <br/>
        /// Error of a single entry is logged and does not interrupt processing of the other entries.
        /// </summary>
        /// <typeparam name="T">Type of the processed file system entry</typeparam>
        /// <param name="directoryInfo">Directory which content must be processed</param>
        /// <param name="getEntries">Delegate which returns entries matching the passed mask</param>
        /// <param name="processEntry">Delegate which processes a single entry and returns true when something was installed</param>
        /// <param name="entriesDescription">Description of the processed entries which is used in error log records</param>
        /// <returns>True when at least one entry was installed</returns>
        protected bool ProcessEntries<T>(
            DirectoryInfo directoryInfo,
            Func<string, T[]> getEntries,
            Func<T, bool> processEntry,
            string entriesDescription)
            where T : FileSystemInfo
        {
            if (!directoryInfo.Exists)
            {
                return false;
            }

            bool anyProcessed = false;

            using (var progress = new ProgressForm())
            {
                foreach (var mask in Masks)
                {
                    var entries = getEntries(mask);

                    progress.SetMax(entries.Length);

                    for (int i = 0; i < entries.Length; i++)
                    {
                        progress.SetProgress(i);

                        var entry = entries[i];

                        if (!entry.Exists)
                        {
                            continue;
                        }

                        progress.SetInfo(entry.Name);

                        try
                        {
                            if (processEntry(entry))
                            {
                                anyProcessed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error("An error occured while parse by " + entriesDescription + ". error\r\n" + ex);
                        }
                    }
                }
            }

            return anyProcessed;
        }
    }
}
