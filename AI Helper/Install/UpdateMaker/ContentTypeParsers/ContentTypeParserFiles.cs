using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    partial class UpdateMaker
    {
        class ContentTypeParserFiles : ContentTypeParserBase
        {
            public ContentTypeParserFiles(INIFile ini) : base(ini)
            {
            }

            protected override string PathsKeyName => UpdateMaker.FilesKey;

            protected override void Copy(HashSet<string> paths, string gameDirPath, string updateDirPath)
            {
                CopyFiles(paths, gameDirPath, updateDirPath);
            }

            protected override string BlacklistKeyName => "BlacklistFiles";

            public override string RemoveKeyName => "RemoveFiles";

            private void CopyFiles(HashSet<string> parameterFiles, string parameterGameDir, string parameterUpdateDir)
            {
                if (parameterFiles == null || !Directory.Exists(parameterGameDir))
                {
                    return;
                }

                bool getAll = false;
                bool firstCheck = true;
                foreach (var subPath in parameterFiles)
                {
                    if (firstCheck)
                    {
                        firstCheck = false;

                        if (string.IsNullOrWhiteSpace(subPath)) // first subpath is invalid
                        {
                            break;
                        }
                        else if (subPath == "*") // parameter * means copy all files
                        {
                            getAll = true;
                            break;
                        }
                    }

                    if (CopyFileBySubPath(subPath, parameterGameDir, parameterUpdateDir))
                    {
                        UpdateMaker.IsAnyFileCopied = true;
                    }
                }

                if (getAll)
                {
                    foreach (var file in Directory.EnumerateFiles(parameterGameDir, "*"))
                    {
                        if (UseBlacklist && file.ContainsAnyFrom(UpdateMaker.blacklist)) continue;

                        var subPath = file.Replace(parameterGameDir, string.Empty);
                        if (CopyFileBySubPath(subPath, parameterGameDir, parameterUpdateDir, copyAll: true))
                        {
                            UpdateMaker.IsAnyFileCopied = true;
                        }
                    }
                }
            }

            private bool CopyFileBySubPath(string subPath, string sourceDir, string targetDir, bool copyAll = false)
            {
                if (UseBlacklist && copyAll && UpdateMaker.blacklist.Contains(UpdateMaker.DirName + "\\" + subPath)) // dirname before because blacklist record is starts with sourceDir name
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

            protected override IEnumerable<string> EnumerateSubpathsToRemove(HashSet<string> paths, string gameDirPath, string updateDirPath)
            {
                var updateGameDir = Path.Combine(updateDirPath, "Games", ManageSettings.CurrentGameDirName);

                foreach (var subpath in paths)
                {
                    if (string.IsNullOrWhiteSpace(subpath))
                    {
                        continue;
                    }

                    var dirName = UpdateMaker.DirName + "\\";
                    if (IncludedList.Contains(subpath) || (subpath.StartsWith(dirName)) && IncludedList.Contains(subpath.Substring(dirName.Length)))
                    {
                        continue; // skip if file is in included files list
                    }

                    var blacklistedSubPath = updateGameDir + Path.DirectorySeparatorChar + subpath;
                    if (File.Exists(blacklistedSubPath))
                    {
                        //File.Delete(blacklistedSubPath);
                        yield return subpath;
                    }
                }
            }
        }
    }
}
