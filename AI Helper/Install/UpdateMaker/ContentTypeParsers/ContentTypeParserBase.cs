using INIFileMan;
using System.Collections.Generic;
using System.Linq;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Base class of the parsers which are copying content of the selected content type
    /// from the game dir to the update dir
    /// </summary>
    abstract class ContentTypeParserBase
    {
        /// <summary>
        /// ini file with the update maker parameters
        /// </summary>
        protected readonly INIFile Ini;

        /// <summary>
        /// Update maker of the content type which is processed now
        /// </summary>
        internal UpdateMakerBase Maker { get; set; }

        protected ContentTypeParserBase(INIFile ini)
        {
            Ini = ini;
        }

        /// <summary>
        /// True when the paths key of this content type is set in the ini
        /// </summary>
        public bool IsNeedToCopy => Ini.KeyExists(PathsKeyName);

        /// <summary>
        /// Key name with the list of paths which must be copied
        /// </summary>
        protected abstract string PathsKeyName { get; }

        /// <summary>
        /// Key name with the list of paths which must be excluded from the copy
        /// </summary>
        protected abstract string BlacklistKeyName { get; }

        /// <summary>
        /// Key name with the list of paths which must be removed from the game during the update
        /// </summary>
        public abstract string RemoveKeyName { get; }

        /// <summary>
        /// Paths which must be removed from the game during the update
        /// </summary>
        public HashSet<string> RemoveList { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Paths which are copied to the update dir
        /// </summary>
        public HashSet<string> IncludedList { get; private set; } = new HashSet<string>();

        /// <summary>
        /// True when the blacklist from the ini must be used
        /// </summary>
        protected bool UseBlacklist { get; private set; }

        /// <summary>
        /// Copies content of the selected content type from <paramref name="gameDirPath"/> to <paramref name="updateDirPath"/>
        /// </summary>
        /// <param name="gameDirPath">Directory of the content type in the game dir</param>
        /// <param name="updateDirPath">Directory of the content type in the update dir</param>
        public void CopyIncludedContent(string gameDirPath, string updateDirPath)
        {
            IncludedList = SplitPaths(Ini.GetKey("", PathsKeyName));
            RemoveList = SplitPaths(Ini.GetKey("", BlacklistKeyName));

            // maker must always use the current remove list because the same parser instance is reused for every maker
            Maker.Blacklist = RemoveList;
            UseBlacklist = RemoveList.Count > 0;

            if (IncludedList.Count > 0)
            {
                CopyPaths(IncludedList, gameDirPath, updateDirPath);
            }
        }

        /// <summary>
        /// Copies <paramref name="paths"/> of the content type from <paramref name="gameDirPath"/> to <paramref name="updateDirPath"/>
        /// </summary>
        protected abstract void CopyPaths(HashSet<string> paths, string gameDirPath, string updateDirPath);

        /// <summary>
        /// True when <paramref name="paths"/> requires copying of the whole content type dir
        /// </summary>
        protected static bool IsCopyAllRequested(HashSet<string> paths)
        {
            return paths.Contains("*");
        }

        /// <summary>
        /// Splits comma separated list of paths and skips empty records
        /// </summary>
        private static HashSet<string> SplitPaths(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : value.Split(',').Where(path => !string.IsNullOrWhiteSpace(path)).ToHashSet();
        }
    }
}
