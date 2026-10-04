using INIFileMan;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Copies the content of one content type of the game dir to the update dir
    /// </summary>
    /// <remarks>
    /// A parser holds no per-run state: the content type it works on is passed in on every call,
    /// so one instance can be reused for every maker without one maker's data leaking into another.
    /// </remarks>
    abstract class ContentTypeParserBase
    {
        private readonly INIFile _ini;

        protected ContentTypeParserBase(INIFile ini)
        {
            _ini = ini;
        }

        /// <summary>
        /// Key with the list of paths which must be copied for the content type
        /// </summary>
        protected abstract string GetPathsKey(UpdateMakerBase maker);

        /// <summary>
        /// Key with the list of paths which must be excluded from the copy
        /// </summary>
        protected abstract string BlacklistKeyName { get; }

        /// <summary>
        /// Key with the list of paths which must be removed from the game during the update
        /// </summary>
        public abstract string RemoveKeyName { get; }

        /// <summary>
        /// True when the update maker ini selects any content of <paramref name="maker"/>'s content type
        /// </summary>
        public bool IsSelectedFor(UpdateMakerBase maker)
        {
            return _ini.KeyExists(GetPathsKey(maker));
        }

        /// <summary>
        /// Paths which must be removed from the game during the update
        /// </summary>
        /// <remarks>
        /// The blacklist doubles as the removal list: a path excluded from the update is also removed
        /// from the game, so the game can not keep a stale copy next to the updated one.
        /// Records are relative to the game dir and start with the content type dir name.
        /// </remarks>
        public HashSet<string> ReadRemoveList()
        {
            return SplitPaths(_ini.GetKey("", BlacklistKeyName));
        }

        /// <summary>
        /// Copies the content of <paramref name="maker"/>'s content type selected in the ini
        /// from <paramref name="gameDirPath"/> to <paramref name="updateDirPath"/>
        /// </summary>
        /// <param name="maker">Content type which is processed now</param>
        /// <param name="gameDirPath">Directory of the content type in the game dir</param>
        /// <param name="updateDirPath">Directory of the content type in the update dir</param>
        /// <returns>True when at least one entry was copied</returns>
        public bool CopyContent(UpdateMakerBase maker, string gameDirPath, string updateDirPath)
        {
            var includedPaths = SplitPaths(_ini.GetKey("", GetPathsKey(maker)));
            if (includedPaths.Count == 0)
            {
                return false;
            }

            return CopyPaths(includedPaths, ReadRemoveList(), maker, gameDirPath, updateDirPath);
        }

        /// <summary>
        /// Copies <paramref name="paths"/> of the content type, skipping the <paramref name="blacklist"/> records
        /// </summary>
        /// <returns>True when at least one entry was copied</returns>
        protected abstract bool CopyPaths(HashSet<string> paths, HashSet<string> blacklist, UpdateMakerBase maker, string gameDirPath, string updateDirPath);

        /// <summary>
        /// True when <paramref name="paths"/> requires copying of the whole content type dir
        /// </summary>
        protected static bool IsCopyAllRequested(HashSet<string> paths)
        {
            return paths.Contains("*");
        }

        /// <summary>
        /// True when the blacklist excludes the <paramref name="subPath"/> of <paramref name="maker"/>'s content type
        /// </summary>
        protected static bool IsBlacklisted(HashSet<string> blacklist, UpdateMakerBase maker, string subPath)
        {
            // records are relative to the game dir, so the content type dir name is their first segment
            return blacklist.Contains(maker.DirName + Path.DirectorySeparatorChar + subPath);
        }

        /// <summary>
        /// Splits comma separated list of paths and skips empty records
        /// </summary>
        private static HashSet<string> SplitPaths(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new HashSet<string>()
                : value.Split(',').Select(path => path.Trim()).Where(path => path.Length > 0).ToHashSet();
        }
    }
}
