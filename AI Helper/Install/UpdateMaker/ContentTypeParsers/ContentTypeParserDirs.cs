using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Install.UpdateMaker
{
    partial class UpdateMaker
    {
        class ContentTypeParserDirs : ContentTypeParserBase
        {
            public ContentTypeParserDirs(INIFile ini) : base(ini)
            {
            }

            protected override string PathsKeyName => UpdateMaker.DirsKey;

            protected override void Copy(HashSet<string> paths, string gameDirPath, string updateDirPath)
            {
                CopyDirs(paths, gameDirPath, updateDirPath);
            }

            protected override string BlacklistKeyName => "BlacklistDirs";

            public override string RemoveKeyName => "RemoveDirs";

            private void CopyDirs(HashSet<string> parameterDirs, string parameterGameDir, string parameterUpdateDir)
            {
                bool getAll = false;
                bool firstCheck = true;
                foreach (var subPath in parameterDirs)
                {
                    if (firstCheck)
                    {
                        firstCheck = false;

                        if (subPath == null || subPath == "")
                        {
                            break;
                        }
                        else if (subPath == "*")
                        {
                            getAll = true;
                            break;
                        }
                    }

                    if (CopyDirBySubPath(subPath, parameterGameDir, parameterUpdateDir))
                    {
                        UpdateMaker.IsAnyFileCopied = true;
                    }
                }

                if (getAll)
                {
                    foreach (var dir in Directory.EnumerateDirectories(parameterGameDir, "*"))
                    {
                        if (UseBlacklist && dir.ContainsAnyFrom(RemoveList)) continue;

                        var subPath = dir.Replace(parameterGameDir + Path.DirectorySeparatorChar, string.Empty);

                        if (CopyDirBySubPath(subPath, parameterGameDir, parameterUpdateDir))
                        {
                            UpdateMaker.IsAnyFileCopied = true;
                        }
                    }
                }
            }

            private bool CopyDirBySubPath(string subPath, string sourceDir, string targetDir)
            {
                if (UseBlacklist && UpdateMaker.blacklist.Contains(UpdateMaker.DirName + Path.DirectorySeparatorChar + subPath))
                {
                    if (!RemoveList.Contains(subPath)) RemoveList.Add(subPath);
                    return false;
                }

                var path = new DirectoryInfo(sourceDir + Path.DirectorySeparatorChar + subPath);

                if (!path.Exists)
                {
                    return false;
                }

                var targetPath = new DirectoryInfo(targetDir + Path.DirectorySeparatorChar + subPath);
                targetPath.Parent.Create();

                path.CopyAll(targetPath, exclusions: UpdateMaker.blacklist);

                return true;
            }

            protected override IEnumerable<string> EnumerateSubpathsToRemove(HashSet<string> paths, string gameDirPath, string updateDirPath)
            {
                foreach (var subpath in paths)
                {
                    var blacklistedSubPath = gameDirPath + Path.DirectorySeparatorChar + subpath;
                    if (!Directory.Exists(blacklistedSubPath)) continue;

                    //Directory.Delete(blacklistedSubPath, true);
                    yield return subpath;
                }
            }
        }
    }
}
