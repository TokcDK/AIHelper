using AIHelper.Manage;
using AIHelper.Manage.Update;
using INIFileMan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AIHelper.Install.UpdateMaker
{
    class UpdateMaker
    {
        abstract class ContentTypeParser
        {
            protected INIFileMan.INIFile Ini;
            internal UpdateMakerBase UpdateMaker;

            protected ContentTypeParser(INIFileMan.INIFile ini)
            {
                Ini = ini;
            }

            public bool IsNeedToCopy { get => Ini.KeyExists("", PathsKeyName); }
            protected abstract string PathsKeyName { get; }
            protected abstract string BlacklistKeyName { get; }
            public abstract string RemoveKeyName { get; }
            public HashSet<string> RemoveList = new HashSet<string>();
            public HashSet<string> IncludedList = new HashSet<string>();
            protected bool UseBlacklist = false;

            public void Copy(string gameDirPath, string updateDirPath)
            {
                var includedPathsValue = Ini.GetKey("", PathsKeyName);
                if (includedPathsValue != null)
                {
                    IncludedList = includedPathsValue.Split(',').Distinct().ToHashSet();
                }

                var pathsValue = Ini.GetKey("", BlacklistKeyName);
                if (pathsValue != null)
                {
                    RemoveList = pathsValue.Split(',').ToHashSet();
                    UpdateMaker.blacklist = RemoveList;
                    if (RemoveList.Count > 0)
                    {
                        UseBlacklist = true;

                        foreach (var subpath in EnumerateSubpathsToRemove(RemoveList, gameDirPath, updateDirPath))
                        {
                            var blacklistedSubPath = gameDirPath + Path.DirectorySeparatorChar + subpath;
                            if (Directory.Exists(blacklistedSubPath) && !RemoveList.Contains(subpath))
                            {
                                if(!RemoveList.Contains(subpath)) RemoveList.Add(subpath);
                            }
                        }
                    }
                }

                if (IncludedList.Count > 0) Copy(IncludedList, gameDirPath, updateDirPath);
            }
            protected abstract void Copy(HashSet<string> paths, string gameDirPath, string updateDirPath);

            protected abstract IEnumerable<string> EnumerateSubpathsToRemove(HashSet<string> paths, string gameDirPath,string updateDirPath);
        }
        class ContentTypeParserDirs : ContentTypeParser
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
        class ContentTypeParserFiles : ContentTypeParser
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

        Dictionary<string, string> _gameupdatekeys = new Dictionary<string, string>();

        public bool MakeUpdate()
        {
            var updateMakeInfoFilePath = Path.Combine(ManageSettings.CurrentGameDirPath, "makeupdate.ini");
            if (!File.Exists(updateMakeInfoFilePath))
            {
                return false;
            }

            List<UpdateMakerBase> parameters = new List<UpdateMakerBase>()
            {
                new UpdateMakerMO(),
                new UpdateMakerMods(),
                new UpdateMakerData()
            };
            
            var infoIni = ManageIni.GetINIFile(updateMakeInfoFilePath);
            var updateDir = Path.Combine(ManageSettings.CurrentGameDirPath, "Updates", "Update");
            if (!InitUpdateDir(updateDir)) return false;

            var contentTypeParsers = new ContentTypeParser[]
            {
                new ContentTypeParserDirs(infoIni),
                new ContentTypeParserFiles(infoIni)
            };

            CopyAndAddUpdatePaths(parameters, contentTypeParsers, updateDir);

            WriteUpdateIni(infoIni, updateDir, contentTypeParsers);

            Process.Start(updateDir);

            return true;
        }

        private bool InitUpdateDir(string updateDir)
        {
            var dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;
            if (Directory.Exists(updateDir))
            {
                try
                {
                    Directory.Move(updateDir, updateDir + dateTimeSuffix);
                }
                catch
                {
                    return false;
                }
            }
            Directory.CreateDirectory(updateDir);

            return true;
        }

        private void CopyAndAddUpdatePaths(List<UpdateMakerBase> updateMakers, ContentTypeParser[] contentTypeParsers, string updateDir)
        {
            var updateGameDir = Path.Combine(updateDir, "Games", ManageSettings.CurrentGameDirName);

            foreach (var updateMaker in updateMakers)
            {
                var parameterGameDir = Path.Combine(ManageSettings.CurrentGameDirPath, updateMaker.DirName);
                var parameterUpdateDir = Path.Combine(updateGameDir, updateMaker.DirName);

                foreach (var contentTypeParser in contentTypeParsers)
                {
                    contentTypeParser.UpdateMaker = updateMaker;

                    if (!contentTypeParser.IsNeedToCopy) continue;

                    contentTypeParser.Copy(parameterGameDir, parameterUpdateDir);

                    _gameupdatekeys.Add("Update" + updateMaker.DirName, updateMaker.IsAnyFileCopied.ToString().ToLowerInvariant());
                }
            }
        }

        private void WriteUpdateIni(INIFileMan.INIFile ini, string updateDir, ContentTypeParser[] contentTypeParsers)
        {
            var gameupdateini = ManageIni.GetINIFile(Path.Combine(updateDir, ManageSettings.GameUpdateInstallerIniFileName));
            gameupdateini.SetKey("", "GameFolderName", ManageSettings.CurrentGameDirName);
            gameupdateini.SetKey("", "IsRoot", "true");
            // add keys
            foreach (var parameter in _gameupdatekeys)
            {
                gameupdateini.SetKey("", parameter.Key, parameter.Value);
            }
            // add other missing keys and their default values
            foreach (var parameter in new Types.Directories.GameUpdateInfo().Keys)
            {
                if (!gameupdateini.KeyExists(parameter.Key, ""))
                {
                    gameupdateini.SetKey("", parameter.Key, parameter.Value);
                }
            }
            // set removelists
            foreach (var contentTypeParser in contentTypeParsers)
            {
                gameupdateini.SetKey("", contentTypeParser.RemoveKeyName, string.Join(",", contentTypeParser.RemoveList));
            }
            // set defaults from maker
            if (ini.SectionExistsAndNotEmpty("Default"))
            {
                foreach (var parameter in ini.GetSectionKeyValuePairs("Default"))
                {
                    gameupdateini.SetKey("", parameter.Key, parameter.Value);
                }
            }

            gameupdateini.WriteFile();
        }
    }
}
