using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Copies files of the selected content type from the game dir to the update dir
    /// </summary>
    class ContentTypeParserFiles : ContentTypeParserBase
    {
        public ContentTypeParserFiles(INIFile ini) : base(ini)
        {
        }

        protected override string PathsKeyName => Maker.FilesKey;

        protected override string BlacklistKeyName => "BlacklistFiles";

        public override string RemoveKeyName => "RemoveFiles";

        protected override void CopyPaths(HashSet<string> paths, string gameDirPath, string updateDirPath)
        {
            if (!Directory.Exists(gameDirPath))
            {
                return;
            }

            if (IsCopyAllRequested(paths))
            {
                CopyAllFiles(gameDirPath, updateDirPath);
                return;
            }

            foreach (var subPath in paths)
            {
                if (CopyFileBySubPath(subPath, gameDirPath, updateDirPath))
                {
                    Maker.IsAnyFileCopied = true;
                }
            }
        }

        private void CopyAllFiles(string gameDirPath, string updateDirPath)
        {
            foreach (var file in Directory.EnumerateFiles(gameDirPath, "*"))
            {
                if (UseBlacklist && file.ContainsAnyFrom(Maker.Blacklist)) continue;

                var subPath = file.Replace(gameDirPath + Path.DirectorySeparatorChar, string.Empty);

                if (CopyFileBySubPath(subPath, gameDirPath, updateDirPath, copyAll: true))
                {
                    Maker.IsAnyFileCopied = true;
                }
            }
        }

        private bool CopyFileBySubPath(string subPath, string sourceDir, string targetDir, bool copyAll = false)
        {
            // DirName is placed in the beginning of the blacklist record because records are relative to the game dir
            if (UseBlacklist && copyAll && Maker.Blacklist.Contains(Maker.DirName + Path.DirectorySeparatorChar + subPath))
            {
                RemoveList.Add(subPath);
                return false;
            }

            var path = new FileInfo(sourceDir + Path.DirectorySeparatorChar + subPath);

            if (!path.Exists)
            {
                return false;
            }

            var targetPath = new FileInfo(targetDir + Path.DirectorySeparatorChar + subPath);
            targetPath.Directory.Create();

            path.CopyTo(targetPath.FullName);

            return true;
        }
    }
}
