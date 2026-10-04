using INIFileMan;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Copies files of a content type from the game dir to the update dir
    /// </summary>
    class ContentTypeParserFiles : ContentTypeParserBase
    {
        public ContentTypeParserFiles(INIFile ini) : base(ini)
        {
        }

        protected override string GetPathsKey(UpdateMakerBase maker)
        {
            return maker.FilesKey;
        }

        protected override string BlacklistKeyName => "BlacklistFiles";

        public override string RemoveKeyName => "RemoveFiles";

        protected override bool CopyPaths(HashSet<string> paths, HashSet<string> blacklist, UpdateMakerBase maker, string gameDirPath, string updateDirPath)
        {
            if (!Directory.Exists(gameDirPath))
            {
                return false;
            }

            if (IsCopyAllRequested(paths))
            {
                return CopyAllFiles(blacklist, maker, gameDirPath, updateDirPath);
            }

            // an explicitly selected file wins over the blacklist
            var anyCopied = false;
            foreach (var subPath in paths)
            {
                if (CopyFileBySubPath(subPath, gameDirPath, updateDirPath))
                {
                    anyCopied = true;
                }
            }

            return anyCopied;
        }

        /// <summary>
        /// Copies every file of the content type which is not blacklisted
        /// </summary>
        private static bool CopyAllFiles(HashSet<string> blacklist, UpdateMakerBase maker, string gameDirPath, string updateDirPath)
        {
            var anyCopied = false;

            foreach (var filePath in Directory.EnumerateFiles(gameDirPath, "*"))
            {
                var subPath = Path.GetFileName(filePath);
                if (IsBlacklisted(blacklist, maker, subPath))
                {
                    continue;
                }

                if (CopyFileBySubPath(subPath, gameDirPath, updateDirPath))
                {
                    anyCopied = true;
                }
            }

            return anyCopied;
        }

        /// <summary>
        /// Copies the <paramref name="subPath"/> file of the content type to the update dir
        /// </summary>
        private static bool CopyFileBySubPath(string subPath, string sourceDir, string targetDir)
        {
            var source = new FileInfo(Path.Combine(sourceDir, subPath));
            if (!source.Exists)
            {
                return false;
            }

            var targetPath = Path.Combine(targetDir, subPath);
            if (!File.Exists(targetPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                source.CopyTo(targetPath);
            }

            return true;
        }
    }
}
