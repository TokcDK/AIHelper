using AIHelper.Manage;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AIHelper.Install.UpdateMaker
{
    class UpdateMaker
    {
        UpdateMakerBase _parameter;
        Dictionary<string, string> _gameupdatekeys;
        bool _useBlacklist;
        List<string> _removeDirsList;
        List<string> _removeFilesList;

        public bool MakeUpdate()
        {
            var updateMakeInfoFilePath = Path.Combine(ManageSettings.GetCurrentGameDirPath(), "makeupdate.ini");
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
            var blacklistDirs = blValueDirs.Split(',').ToHashSet();
            var blValueFiles = infoIni.GetKey("", "BlacklistFiles");
            var blacklistFiles = blValueFiles.Split(',').ToHashSet();

            var dateTimeSuffix = ManageSettings.GetDateTimeBasedSuffix();

            var updateDir = Path.Combine(ManageSettings.GetCurrentGameDirPath(), "Updates", "Update");
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

            var updateGameDir = Path.Combine(updateDir, "Games", ManageSettings.GetCurrentGameFolderName());

            foreach (var parameter in parameters)
            {
                _parameter = parameter;

                var parameterGameDir = Path.Combine(ManageSettings.GetCurrentGameDirPath(), parameter.DirName);
                var parameterUpdateDir = Path.Combine(updateGameDir, parameter.DirName);

                if (infoIni.KeyExists(parameter.DirsKey))
                {
                    var parameterDirs = infoIni.GetKey("", parameter.DirsKey).Split(',');

                    _parameter.blacklist = blacklistDirs;
                    _useBlacklist = _parameter.blacklist != null && !string.IsNullOrWhiteSpace(blValueDirs) && _parameter.blacklist.Count > 0;

                    ParseDirs(parameterDirs, parameterGameDir, parameterUpdateDir);

                    _gameupdatekeys.Add("Update" + parameter.DirName, parameter.Ret.ToString().ToLowerInvariant());

                    if (_useBlacklist && parameter.Ret)
                    {
                        foreach (var subpath in _parameter.blacklist)
                        {
                            var blacklistedSubPath = updateGameDir + Path.DirectorySeparatorChar + subpath;
                            if (Directory.Exists(blacklistedSubPath))
                            {
                                Directory.Delete(blacklistedSubPath, true);
                                _removeDirsList.Add(subpath);
                            }
                        }
                    }
                }

                if (infoIni.KeyExists(parameter.FilesKey))
                {
                    var parameterFiles = infoIni.GetKey("", parameter.FilesKey).Split(',');

                    _parameter.blacklist = blacklistFiles;
                    _useBlacklist = _parameter.blacklist != null && !string.IsNullOrWhiteSpace(blValueFiles) && _parameter.blacklist.Count > 0;

                    ParseFiles(parameterFiles, parameterGameDir, parameterUpdateDir);

                    var key = "Update" + parameter.DirName;
                    if (!_gameupdatekeys.ContainsKey(key))
                    {
                        _gameupdatekeys.Add(key, parameter.Ret.ToString().ToLowerInvariant());
                    }
                    else if (_gameupdatekeys[key] == "false")
                    {
                        _gameupdatekeys[key] = parameter.Ret.ToString().ToLowerInvariant();
                    }

                    if (_useBlacklist && parameter.Ret)
                    {
                        foreach (var subpath in _parameter.blacklist)
                        {
                            var blacklistedSubPath = updateGameDir + Path.DirectorySeparatorChar + subpath;
                            if (File.Exists(blacklistedSubPath))
                            {
                                File.Delete(blacklistedSubPath);
                                _removeFilesList.Add(subpath);
                            }
                        }
                    }
                }
            }

            var gameupdateini = ManageIni.GetINIFile(Path.Combine(updateDir, "gameupdate.ini"));
            gameupdateini.SetKey("", "GameFolderName", ManageSettings.GetCurrentGameFolderName());
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

        private void ParseFiles(string[] parameterFiles, string parameterGameDir, string parameterUpdateDir)
        {
            if (string.IsNullOrWhiteSpace(parameterFiles[0]) || !Directory.Exists(parameterGameDir))
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

                if (CopyFileBySubPath(subPath, parameterGameDir, parameterUpdateDir))
                {
                    _parameter.Ret = true;
                }
            }

            if (getAll)
            {
                foreach (var dir in Directory.EnumerateFiles(parameterGameDir, "*"))
                {
                    var subPath = dir.Replace(parameterGameDir, string.Empty);
                    if (CopyFileBySubPath(subPath, parameterGameDir, parameterUpdateDir))
                    {
                        _parameter.Ret = true;
                    }
                }
            }
        }

        private bool CopyFileBySubPath(string subPath, string sourceDir, string targetDir)
        {
            if (_useBlacklist && _parameter.blacklist.Contains(_parameter.DirName + Path.DirectorySeparatorChar + subPath))
            {
                _removeFilesList.Add(subPath);
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

        private void ParseDirs(string[] parameterDirs, string parameterGameDir, string parameterUpdateDir)
        {
            if (string.IsNullOrWhiteSpace(parameterDirs[0]) || !Directory.Exists(parameterGameDir))
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
                    _parameter.Ret = true;
                }
            }

            if (getAll)
            {
                foreach (var dir in Directory.EnumerateDirectories(parameterGameDir, "*"))
                {
                    var subPath = dir.Replace(parameterGameDir + Path.DirectorySeparatorChar, string.Empty);

                    if (CopyDirBySubPath(subPath, parameterGameDir, parameterUpdateDir))
                    {
                        _parameter.Ret = true;
                    }
                }
            }
        }

        private bool CopyDirBySubPath(string subPath, string sourceDir, string targetDir)
        {
            if (_useBlacklist && _parameter.blacklist.Contains(_parameter.DirName + Path.DirectorySeparatorChar + subPath))
            {
                _removeDirsList.Add(subPath);
                return false;
            }

            var path = new DirectoryInfo(sourceDir + Path.DirectorySeparatorChar + subPath);

            if (!path.Exists)
            {
                return false;
            }

            var targetPath = new DirectoryInfo(targetDir + Path.DirectorySeparatorChar + subPath);
            targetPath.Parent.Create();

            path.CopyAll(targetPath);

            if (subPath == "GameUserData")
            {

            }

            return true;
        }
    }
}
