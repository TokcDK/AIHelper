using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Copies dirs of the selected content type from the game dir to the update dir
    /// </summary>
    class ContentTypeParserDirs : ContentTypeParserBase
    {
        public ContentTypeParserDirs(INIFile ini) : base(ini)
        {
        }

        protected override string PathsKeyName => Maker.DirsKey;

        protected override string BlacklistKeyName => "BlacklistDirs";

        public override string RemoveKeyName => "RemoveDirs";

        protected override void CopyPaths(HashSet<string> paths, string gameDirPath, string updateDirPath)
        {
            if (IsCopyAllRequested(paths))
            {
                CopyAllDirs(gameDirPath, updateDirPath);
                return;
            }

            foreach (var subPath in paths)
            {
                if (CopyDirBySubPath(subPath, gameDirPath, updateDirPath))
                {
                    Maker.IsAnyFileCopied = true;
                }
            }
        }

        private void CopyAllDirs(string gameDirPath, string updateDirPath)
        {
            foreach (var dir in Directory.EnumerateDirectories(gameDirPath, "*"))
            {
                if (UseBlacklist && dir.ContainsAnyFrom(RemoveList)) continue;

                var subPath = dir.Replace(gameDirPath + Path.DirectorySeparatorChar, string.Empty);

                if (CopyDirBySubPath(subPath, gameDirPath, updateDirPath))
                {
                    Maker.IsAnyFileCopied = true;
                }
            }
        }

        private bool CopyDirBySubPath(string subPath, string sourceDir, string targetDir)
        {
            if (UseBlacklist && Maker.Blacklist.Contains(Maker.DirName + Path.DirectorySeparatorChar + subPath))
            {
                RemoveList.Add(subPath);
                return false;
            }

            var path = new DirectoryInfo(sourceDir + Path.DirectorySeparatorChar + subPath);

            if (!path.Exists)
            {
                return false;
            }

            var targetPath = new DirectoryInfo(targetDir + Path.DirectorySeparatorChar + subPath);
            targetPath.Parent.Create();

            path.CopyAll(targetPath, exclusions: Maker.Blacklist);

            return true;
        }
    }
}
