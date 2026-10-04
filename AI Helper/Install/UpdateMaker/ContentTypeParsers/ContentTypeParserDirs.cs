using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Copies dirs of a content type from the game dir to the update dir
    /// </summary>
    class ContentTypeParserDirs : ContentTypeParserBase
    {
        public ContentTypeParserDirs(INIFile ini) : base(ini)
        {
        }

        protected override string GetPathsKey(UpdateMakerBase maker)
        {
            return maker.DirsKey;
        }

        protected override string BlacklistKeyName => "BlacklistDirs";

        public override string RemoveKeyName => "RemoveDirs";

        protected override bool CopyPaths(HashSet<string> paths, HashSet<string> blacklist, UpdateMakerBase maker, string gameDirPath, string updateDirPath)
        {
            if (IsCopyAllRequested(paths))
            {
                return CopyAllDirs(blacklist, maker, gameDirPath, updateDirPath);
            }

            // an explicitly selected dir wins over the blacklist, exactly like an explicitly selected file
            var anyCopied = false;
            foreach (var subPath in paths)
            {
                if (CopyDirBySubPath(subPath, blacklist, gameDirPath, updateDirPath))
                {
                    anyCopied = true;
                }
            }

            return anyCopied;
        }

        /// <summary>
        /// Copies every dir of the content type which is not blacklisted
        /// </summary>
        private static bool CopyAllDirs(HashSet<string> blacklist, UpdateMakerBase maker, string gameDirPath, string updateDirPath)
        {
            var anyCopied = false;

            foreach (var dirPath in Directory.EnumerateDirectories(gameDirPath, "*"))
            {
                var subPath = Path.GetFileName(dirPath);
                if (IsBlacklisted(blacklist, maker, subPath))
                {
                    continue;
                }

                if (CopyDirBySubPath(subPath, blacklist, gameDirPath, updateDirPath))
                {
                    anyCopied = true;
                }
            }

            return anyCopied;
        }

        /// <summary>
        /// Copies the <paramref name="subPath"/> dir of the content type to the update dir
        /// </summary>
        private static bool CopyDirBySubPath(string subPath, HashSet<string> blacklist, string sourceDir, string targetDir)
        {
            var source = new DirectoryInfo(Path.Combine(sourceDir, subPath));
            if (!source.Exists)
            {
                return false;
            }

            // CopyAll creates the target dir itself and skips the nested entries of the blacklist
            source.CopyAll(new DirectoryInfo(Path.Combine(targetDir, subPath)), exclusions: blacklist);

            return true;
        }
    }
}
