using AIHelper.Manage;
using INIFileMan;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AIHelper.Install.UpdateMaker
{
    class UpdateMaker
    {
        abstract class ContentTypeParser
        {
            protected INIFileMan.INIFile Ini;
            protected UpdateMakerBase UpdateMaker;

            protected ContentTypeParser(INIFileMan.INIFile ini, UpdateMakerBase updateMaker)
            {
                Ini = ini;
                UpdateMaker = updateMaker;
            }

            public bool IsNeedToCopy { get => Ini.KeyExists("", PathsKeyName); }
            protected abstract string PathsKeyName { get; }
            protected abstract string BlacklistKeyName { get; }
            public abstract string RemoveKeyName { get; }
            public HashSet<string> RemoveList = new HashSet<string>();
            protected bool UseBlacklist = false;

            public void Copy(string gameDirPath, string updateDirPath)
            {
                var pathsValue = Ini.GetKey("", BlacklistKeyName);
                if (pathsValue != null)
                {
                    var blacklistedPaths = pathsValue.Split(',').ToHashSet();
                    if (blacklistedPaths.Count > 0)
                    {
                        UseBlacklist = true;

                        foreach (var subpath in blacklistedPaths)
                        {
                            var blacklistedSubPath = gameDirPath + Path.DirectorySeparatorChar + subpath;
                            if (Directory.Exists(blacklistedSubPath) && !RemoveList.Contains(subpath))
                            {
                                RemoveList.Add(subpath);
                            }
                        }
                    }
                }

                var paths = Ini.GetKey("", PathsKeyName).Split(',').Distinct().ToHashSet();
                Copy(paths, gameDirPath, updateDirPath);
            }
            protected abstract void Copy(HashSet<string> paths, string gameDirPath, string updateDirPath);

        }
        class ContentTypeParserDirs : ContentTypeParser
        {
            public ContentTypeParserDirs(INIFile ini, UpdateMakerBase updateMaker) : base(ini, updateMaker)
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
                if (parameterDirs == null || parameterDirs.Count == 0 || string.IsNullOrWhiteSpace(parameterDirs.First()) || !Directory.Exists(parameterGameDir))
                {
                    return;
                }

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
                        if (_useBlacklist && dir.ContainsAnyFrom(UpdateMaker.blacklist)) continue;

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

                path.CopyAll(targetPath, exclusions: UpdateMaker.blacklist);

                return true;
            }
        }
        class ContentTypeParserFiles : ContentTypeParser
        {
            public ContentTypeParserFiles(INIFile ini, UpdateMakerBase updateMaker) : base(ini, updateMaker)
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
                        if (_useBlacklist && file.ContainsAnyFrom(UpdateMaker.blacklist)) continue;

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
        }


        UpdateMakerBase _parameter;
        Dictionary<string, string> _gameupdatekeys;
        bool _useBlacklist;
        List<string> _removeDirsList;
        List<string> _removeFilesList;

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

            _gameupdatekeys = new Dictionary<string, string>();
            _removeDirsList = new List<string>();
            _removeFilesList = new List<string>();

            var infoIni = ManageIni.GetINIFile(updateMakeInfoFilePath);
            var blValueDirs = infoIni.GetKey("", "BlacklistDirs");
            var blacklistDirs = (blValueDirs == null ? new HashSet<string>() : blValueDirs.Split(',').ToHashSet());
            var blValueFiles = infoIni.GetKey("", "BlacklistFiles");
            var blacklistFiles = (blValueFiles == null ? new HashSet<string>() : blValueFiles.Split(',').ToHashSet());

            var dateTimeSuffix = ManageSettings.DateTimeBasedSuffix;

            var updateDir = Path.Combine(ManageSettings.CurrentGameDirPath, "Updates", "Update");
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

            var updateGameDir = Path.Combine(updateDir, "Games", ManageSettings.CurrentGameDirName);

            foreach (var parameter in parameters)
            {
                _parameter = parameter;

                var parameterGameDir = Path.Combine(ManageSettings.CurrentGameDirPath, parameter.DirName);
                var parameterUpdateDir = Path.Combine(updateGameDir, parameter.DirName);

                foreach(var contentTypeParser in new ContentTypeParser[] 
                { 
                    new ContentTypeParserDirs(infoIni, _parameter), 
                    new ContentTypeParserFiles(infoIni, _parameter) 
                })
                {
                    if (!contentTypeParser.IsNeedToCopy) continue;

                    contentTypeParser.Copy(parameterGameDir, parameterUpdateDir);

                    _gameupdatekeys.Add("Update" + parameter.DirName, parameter.IsAnyFileCopied.ToString().ToLowerInvariant());

                    contentTypeParser.RemoveBlacklisted(parameterGameDir, parameterUpdateDir);
                }

                if (infoIni.KeyExists(parameter.DirsKey))
                {
                    var parameterDirs = infoIni.GetKey("", parameter.DirsKey).Split(',');

                    CopyDirs(parameterDirs, parameterGameDir, parameterUpdateDir);

                    _gameupdatekeys.Add("Update" + parameter.DirName, parameter.IsAnyFileCopied.ToString().ToLowerInvariant());

                    _parameter.blacklist = blacklistDirs;
                    _useBlacklist = !string.IsNullOrWhiteSpace(blValueDirs) && _parameter.blacklist.Count > 0;
                    if (_useBlacklist && parameter.IsAnyFileCopied)
                    {
                        foreach (var subpath in _parameter.blacklist)
                        {
                            var blacklistedSubPath = updateGameDir + Path.DirectorySeparatorChar + subpath;
                            if (Directory.Exists(blacklistedSubPath))
                            {
                                //Directory.Delete(blacklistedSubPath, true);
                                _removeDirsList.Add(subpath);
                            }
                        }
                    }
                }

                if (infoIni.KeyExists(parameter.FilesKey))
                {
                    var parameterFiles = infoIni.GetKey("", parameter.FilesKey).Split(',').ToHashSet();

                    _parameter.blacklist = _parameter.blacklist.Count > 0 ? blacklistFiles.Concat(_parameter.blacklist).ToHashSet() : blacklistFiles;
                    _useBlacklist = !string.IsNullOrWhiteSpace(blValueFiles) && _parameter.blacklist.Count > 0;

                    CopyFiles(parameterFiles, parameterGameDir, parameterUpdateDir);

                    var key = "Update" + parameter.DirName;
                    if (!_gameupdatekeys.ContainsKey(key))
                    {
                        _gameupdatekeys.Add(key, parameter.IsAnyFileCopied.ToString().ToLowerInvariant());
                    }
                    else if (_gameupdatekeys[key] == "false")
                    {
                        _gameupdatekeys[key] = parameter.IsAnyFileCopied.ToString().ToLowerInvariant();
                    }

                    if (_useBlacklist && parameter.IsAnyFileCopied)
                    {
                        foreach (var subpath in _parameter.blacklist)
                        {
                            if (string.IsNullOrWhiteSpace(subpath))
                            {
                                continue;
                            }

                            var dirName = parameter.DirName + "\\";
                            if (parameterFiles.Contains(subpath) || (subpath.StartsWith(dirName)) && parameterFiles.Contains(subpath.Substring(dirName.Length)))
                            {
                                continue; // skip if file is contains in included files list
                            }

                            var blacklistedSubPath = updateGameDir + Path.DirectorySeparatorChar + subpath;
                            if (File.Exists(blacklistedSubPath))
                            {
                                //File.Delete(blacklistedSubPath);
                                _removeFilesList.Add(subpath);
                            }
                        }
                    }
                }
            }

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
            gameupdateini.SetKey("", "RemoveDirs", string.Join(",", _removeDirsList));
            gameupdateini.SetKey("", "RemoveFiles", string.Join(",", _removeFilesList));

            // set defaults from maker
            if (infoIni.SectionExistsAndNotEmpty("Default"))
            {
                foreach (var parameter in infoIni.GetSectionKeyValuePairs("Default"))
                {
                    gameupdateini.SetKey("", parameter.Key, parameter.Value);
                }
            }

            gameupdateini.WriteFile();

            Process.Start(updateDir);

            return true;
        }
    }
}
