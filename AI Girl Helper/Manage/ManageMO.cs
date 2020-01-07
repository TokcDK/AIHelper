using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AIHelper.Manage
{
    class ManageMO
    {
        public static void RedefineGameMOData()
        {
            RedefineCategoriesDat();
            RedefineModOrganizerIni();
        }

        public static void SetModOrganizerINISettingsForTheGame()
        {
            RedefineGameMOData();

            INIFile INI = new INIFile(ManageSettings.GetModOrganizerINIpath());

            SetCommonIniValues(INI);
            SetCustomExecutablesIniValues(INI);
        }

        private static void SetCustomExecutablesIniValues(INIFile INI)
        {
            string[,] IniValues =
                {
                    //customExecutables
                    {
                        ManageSettings.GetCurrentGameEXEName()
                        ,
                        @"1\title"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetDataPath(), ManageSettings.GetCurrentGameEXEName() + ".exe")
                        ,
                        @"1\binary"
                    }
                ,
                    {
                        ManageSettings.GetDataPath()
                        ,
                        @"1\workingDirectory"
                    }
                ,
                    {
                        "true"
                        ,
                        @"1\ownicon"
                    }
                ,
                    {
                        ManageSettings.GetINISettingsEXEName()
                        ,
                        @"2\title"
                    }
                ,
                    {
                        ManageSettings.GetSettingsEXEPath()
                        ,
                        @"2\binary"
                    }
                ,
                    {
                        ManageSettings.GetDataPath()
                        ,
                        @"2\workingDirectory"
                    }
                ,
                    {
                        "true"
                        ,
                        @"2\ownicon"
                    }
                //,
                //    {
                //        "Explore Virtual Folder"
                //        ,
                //        @"3\title"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++", "Explorer++.exe")
                //        ,
                //        @"3\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetDataPath()
                //        ,
                //        @"3\arguments"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++")
                //        ,
                //        @"3\workingDirectory"
                //    }
                //,
                //    {
                //        "true"
                //        ,
                //        @"3\ownicon"
                //    }
                //,
                //    {
                //        "Skyrim"
                //        ,
                //        @"4\title"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
                //        ,
                //        @"4\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetDataPath()
                //        ,
                //        @"4\workingDirectory"
                //    }
                ,
                    {
                        Path.Combine(ManageSettings.GetStudioEXEName())
                        ,
                        @"3\title"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetDataPath(),ManageSettings.GetStudioEXEName()+".exe")
                        ,
                        @"3\binary"
                    }
                ,
                    {
                        ManageSettings.GetDataPath()
                        ,
                        @"3\workingDirectory"
                    }
                ,
                    {
                        "true"
                        ,
                        @"3\ownicon"
                    }
                };

            Dictionary<string, string> IniValuesDict = new Dictionary<string, string>();

            string[,] iniParameters =
                {
                    {
                       "title"
                       ,
                       "rs"
                    }
                ,
                    {
                        "binary"
                    ,
                        "rs"
                    }
                ,
                    {
                        "workingDirectory"
                    ,
                        "rs"
                    }
                ,
                    {
                        "arguments"
                    ,
                        "s"
                    }
                ,
                    {
                        "toolbar"
                    ,
                        "b"
                    }
                ,
                    {
                        "ownicon"
                    ,
                        "b"
                    }
                ,
                    {
                        "steamAppID"
                    ,
                        "s"
                    }
            };

            //заполнить словарь значениями массива строк
            int IniValuesLength = IniValues.Length / 2;//дляна массива, деленое на 2 т.к. каждый элемент состоит из двух
            int exclude = 0;//будет равен индексу, который надо пропустить
            int resultindex = 1;//конечное количество добавленных exe
            int skippedCnt = 0;//счет количества пропущенных, нужно для правильного подсчета resultindex
            for (int i = 0; i < IniValuesLength; i++)
            {
                int curindex = int.Parse(IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0]);
                string parameterName = IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[1];
                if (curindex == exclude)
                {
                    continue;
                }
                else if (exclude != 0)
                {
                    exclude = 0;
                }
                //Если название пустое, то пропускать все значения с таким индексом
                if (parameterName == "title" && IniValues[i, 0].Length == 0)
                {
                    exclude = curindex;
                    skippedCnt++;
                    continue;
                }

                if (curindex - skippedCnt > resultindex && exclude == 0)
                {
                    resultindex++;
                }

                string ResultparameterName = resultindex + @"\" + parameterName;

                if (!IniValuesDict.ContainsKey(ResultparameterName))
                {
                    IniValuesDict.Add(ResultparameterName, IniValues[i, 0]);
                }
            }

            int ExecutablesCount = resultindex;
            //try
            //{
            //    ExecutablesCount = int.Parse(IniValues[IniValuesLength - 1, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0]);
            //}
            //catch
            //{
            //    return;//если не число, выйти
            //}

            var CurrentGame = ManageSettings.GetListOfExistsGames()[Properties.Settings.Default.CurrentGameListIndex];
            if (CurrentGame.GetGameEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetDataPath(), CurrentGame.GetGameEXENameX32() + ".exe")))
            {
                ExecutablesCount++;
                IniValuesDict.Add(ExecutablesCount + @"\title", CurrentGame.GetGameEXENameX32());
                IniValuesDict.Add(ExecutablesCount + @"\binary", Path.Combine(ManageSettings.GetDataPath(), CurrentGame.GetGameEXENameX32() + ".exe"));
                IniValuesDict.Add(ExecutablesCount + @"\workingDirectory", ManageSettings.GetDataPath());
            }
            if (CurrentGame.GetGameStudioEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe")))
            {
                ExecutablesCount++;
                IniValuesDict.Add(ExecutablesCount + @"\title", CurrentGame.GetGameStudioEXENameX32());
                IniValuesDict.Add(ExecutablesCount + @"\binary", Path.Combine(ManageSettings.GetDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe"));
                IniValuesDict.Add(ExecutablesCount + @"\workingDirectory", ManageSettings.GetDataPath());
            }

            string[] pathExclusions = { "BepInEx" + Path.DirectorySeparatorChar + "plugins", "Lec.ExtProtocol", "Common.ExtProtocol.Executor", "UnityCrashHandler64" };

            //Добавление exe из Data
            foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories))
            {
                string exeName = Path.GetFileNameWithoutExtension(exePath);
                if (exeName.Length > 0 && !IniValuesDict.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
                {
                    ExecutablesCount++;
                    IniValuesDict.Add(ExecutablesCount + @"\title", exeName);
                    IniValuesDict.Add(ExecutablesCount + @"\binary", exePath);
                }
            }
            //Добавление exe из Mods
            foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories))
            {
                string exeName = Path.GetFileNameWithoutExtension(exePath);
                if (Path.GetFileNameWithoutExtension(exePath).Length > 0 && !IniValuesDict.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
                {
                    ExecutablesCount++;
                    IniValuesDict.Add(ExecutablesCount + @"\title", exeName);
                    IniValuesDict.Add(ExecutablesCount + @"\binary", exePath);
                }
            }

            int cnt = 1;
            int iniParametersLength = iniParameters.Length / 2;
            while (cnt <= ExecutablesCount)
            {
                for (int i = 0; i < iniParametersLength; i++)
                {
                    string key = cnt + @"\" + iniParameters[i, 0];
                    string IniValue;
                    bool keyExists = IniValuesDict.ContainsKey(key);
                    string subquote = key.EndsWith(@"\arguments") && keyExists ? "\\\"" : string.Empty;
                    if (keyExists)
                    {
                        IniValue = subquote + IniValuesDict[key].Replace(@"\", @"\\") + subquote;
                    }
                    else
                    {
                        IniValue = subquote + (iniParameters[i, 1].Substring(iniParameters[i, 1].Length - 1) == "b" ? "false" : string.Empty) + subquote;
                    }

                    INI.WriteINI("customExecutables", key, IniValue, false);
                }
                cnt++;
            }

            INI.WriteINI("customExecutables", "size", ExecutablesCount.ToString());
        }

        private static void SetCommonIniValues(INIFile INI)
        {
            string[,] IniValues =
                {
                    //General
                    {
                        ManageSettings.GetCurrentGamePath()
                        ,
                        "General"
                        ,
                        "gamePath"
                    }
                //,
                //    {
                //        GetSelectedProfileName()
                //        ,
                //        "General"
                //        ,
                //        @"selected_profile"
                //    }
                ,
                    {
                        "1"
                        ,
                        "General"
                        ,
                        @"selected_executable"
                    }
                //,
                //    //customExecutables
                //    {
                //        SettingsManage.GetCurrentGameEXEName()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"1\title"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
                //        ,
                //        "customExecutables"
                //        ,
                //        @"1\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetDataPath()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"1\workingDirectory"
                //    }
                //,
                //    {
                //        SettingsManage.GetINISettingsEXEName()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"2\title"
                //    }
                //,
                //    {
                //        SettingsManage.GetSettingsEXEPath()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"2\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetDataPath()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"2\workingDirectory"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++", "Explorer++.exe")
                //        ,
                //        "customExecutables"
                //        ,
                //        @"3\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetDataPath()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"3\arguments"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++")
                //        ,
                //        "customExecutables"
                //        ,
                //        @"3\workingDirectory"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetCurrentGamePath(), "TESV.exe")
                //        ,
                //        "customExecutables"
                //        ,
                //        @"4\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetCurrentGamePath()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"4\workingDirectory"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetStudioEXEName())
                //        ,
                //        "customExecutables"
                //        ,
                //        @"5\title"
                //    }
                //,
                //    {
                //        Path.Combine(SettingsManage.GetDataPath(),SettingsManage.GetStudioEXEName()+".exe")
                //        ,
                //        "customExecutables"
                //        ,
                //        @"5\binary"
                //    }
                //,
                //    {
                //        SettingsManage.GetDataPath()
                //        ,
                //        "customExecutables"
                //        ,
                //        @"5\workingDirectory"
                //    }
                ,
                    //Settings
                    {
                        ManageSettings.GetModsPath()
                        ,
                        "Settings"
                        ,
                        @"mod_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGamePath(), "Downloads")
                        ,
                        "Settings"
                        ,
                        @"download_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles")
                        ,
                        "Settings"
                        ,
                        @"profiles_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "overwrite")
                        ,
                        "Settings"
                        ,
                        @"overwrite_directory"
                    }
                ,
                    {
                        CultureInfo.CurrentCulture.Name.Split('-')[0]
                        ,
                        "Settings"
                        ,
                        @"language"
                    }

                };

            int IniValuesLength = IniValues.Length / 3;

            for (int i = 0; i < IniValuesLength; i++)
            {
                string subquote = IniValues[i, 2].EndsWith(@"\arguments") ? "\\\"" : string.Empty;
                string IniValue = subquote + IniValues[i, 0].Replace(@"\", @"\\") + subquote;
                //if (INIManage.GetINIValueIfExist(SettingsManage.GetModOrganizerINIpath(), IniValues[i, 2], IniValues[i, 1]) != IniValue)
                //{
                //    INI.WriteINI(IniValues[i, 1], IniValues[i, 2], IniValue);
                //}
                INI.WriteINI(IniValues[i, 1], IniValues[i, 2], IniValue, false);
            }
        }

        internal static void RedefineModOrganizerIni()
        {
            string[] categoriesdatGameAndLocalPaths = new string[2]
            {
                ManageSettings.GetMOiniPathForSelectedGame()
                ,
                ManageSettings.GetMOiniPath()
            };

            if (
                File.Exists(categoriesdatGameAndLocalPaths[0])
                &&
                (
                !File.Exists(categoriesdatGameAndLocalPaths[1])
                ||
                File.ReadAllText(categoriesdatGameAndLocalPaths[0]) != File.ReadAllText(categoriesdatGameAndLocalPaths[1])
                )
               )
            {
                var moINIinfo = new FileInfo(categoriesdatGameAndLocalPaths[1]);
                if (File.Exists(moINIinfo.FullName) && (!SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLink(moINIinfo) || !SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLinkValid(moINIinfo) || Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(SymbolicLinkSupport.FileInfoExtensions.GetSymbolicLinkTarget(moINIinfo)))) != ManageSettings.GetCurrentGameFolderName()))
                {
                    File.Delete(moINIinfo.FullName);
                }

                ManageFilesFolders.Symlink
                  (
                   categoriesdatGameAndLocalPaths[0]
                   ,
                   categoriesdatGameAndLocalPaths[1]
                   ,
                   true
                  );
            }
        }

        internal static void RedefineCategoriesDat()
        {
            string[] categoriesdatGameAndLocalPaths = new string[2]
            {
                ManageSettings.GetMOcategoriesPathForSelectedGame()
                ,
                ManageSettings.GetMOcategoriesPath()
            };

            if (
                File.Exists(categoriesdatGameAndLocalPaths[0])
                &&
                (
                !File.Exists(categoriesdatGameAndLocalPaths[1])
                ||
                File.ReadAllText(categoriesdatGameAndLocalPaths[0]) != File.ReadAllText(categoriesdatGameAndLocalPaths[1])
                )
               )
            {
                var categoriesDat = new FileInfo(categoriesdatGameAndLocalPaths[1]);
                if (File.Exists(categoriesDat.FullName) && (!SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLink(categoriesDat) || !SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLinkValid(categoriesDat) || Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(SymbolicLinkSupport.FileInfoExtensions.GetSymbolicLinkTarget(categoriesDat)))) != ManageSettings.GetCurrentGameFolderName()))
                {
                    File.Delete(categoriesDat.FullName);
                }

                ManageFilesFolders.Symlink
                  (
                   categoriesdatGameAndLocalPaths[0]
                   ,
                   categoriesdatGameAndLocalPaths[1]
                   ,
                   true
                  );
            }

        }

        public static void ActivateInsertModIfPossible(string modname, bool Activate = true, string modAfterWhichInsert = "", bool PlaceAfter = true)
        {
            if (modname.Length > 0)
            {
                string currentMOprofile = ManageSettings.GetMOSelectedProfileDirPath();

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile, "modlist.txt");

                    ManageINI.InsertLineInFile(profilemodlistpath, (Activate ? "+" : "-") + modname, 1, modAfterWhichInsert, PlaceAfter);
                }
            }
        }

        /// <summary>
        /// Writes required parameters in meta.ini
        /// </summary>
        /// <param name="moddir"></param>
        /// <param name="categoryIDIndex"></param>
        /// <param name="version"></param>
        /// <param name="comments"></param>
        /// <param name="notes"></param>
        public static void WriteMetaINI(string moddir, string categoryIDIndex = "", string version = "", string comments = "", string notes = "")
        {
            if (Directory.Exists(moddir))
            {
                string metaPath = Path.Combine(moddir, "meta.ini");
                INIFile INI = new INIFile(metaPath);

                bool IsKeyExists = INI.KeyExists("category", "General");
                if (!IsKeyExists || (IsKeyExists && categoryIDIndex.Length > 0 && INI.ReadINI("General", "category").Replace("\"", string.Empty).Length == 0))
                {
                    INI.WriteINI("General", "category", "\"" + categoryIDIndex + "\"");
                }

                if (version.Length > 0)
                {
                    INI.WriteINI("General", "version", version);
                }

                INI.WriteINI("General", "gameName", ManageSettings.GETMOCurrentGameName());

                if (comments.Length > 0)
                {
                    INI.WriteINI("General", "comments", comments);
                }

                if (notes.Length > 0)
                {
                    INI.WriteINI("General", "notes", "\"" + notes.Replace(Environment.NewLine, "<br>") + "\"");
                }

                INI.WriteINI("General", "validated", "true");
            }
        }

        public static string[] GetModNamesListFromActiveMOProfile(bool OnlyEnabled = true)
        {
            string currentMOprofile = ManageSettings.GetMOSelectedProfileDirPath();

            if (currentMOprofile.Length > 0)
            {
                string profilemodlistpath = Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile, "modlist.txt");

                if (File.Exists(profilemodlistpath))
                {
                    string[] lines;
                    if (OnlyEnabled)
                    {
                        //все строки с + в начале
                        lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && string.CompareOrdinal(line.Substring(0, 1), "#") != 0 && string.CompareOrdinal(line.Substring(0, 1), "+") == 0).ToArray();
                    }
                    else
                    {
                        //все строки кроме сепараторов
                        lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && string.CompareOrdinal(line.Substring(0, 1), "#") != 0 && (line.Length < 10 || string.CompareOrdinal(line.Substring(line.Length - 10, 10), "_separator") != 0)).ToArray();
                    }
                    //Array.Reverse(lines); //убрал, т.к. дулаю архив с резервными копиями
                    int linesLength = lines.Length;
                    for (int l = 0; l < linesLength; l++)
                    {
                        //remove +-
                        lines[l] = lines[l].Remove(0, 1);
                    }

                    return lines;
                }
            }

            return null;
        }

        public static string GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(string pathInMods, bool IsDir = false)
        {
            //искать путь только для ссылки в Mods или в Data
            if (!ManageStrings.IsStringAContainsStringB(pathInMods, ManageSettings.GetModsPath()) && !ManageStrings.IsStringAContainsStringB(pathInMods, ManageSettings.GetDataPath()))
                return pathInMods;

            //отсеивание первого элемента с именем мода
            string subpath = string.Empty;
            int i = 0;
            foreach (var element in pathInMods.Replace(ManageSettings.GetModsPath(), string.Empty).Split(Path.DirectorySeparatorChar))
            {
                if (i > 1)
                {
                    subpath += i > 2 ? Path.DirectorySeparatorChar.ToString() : string.Empty;
                    subpath += element;
                }
                i++;
            }

            if (!Properties.Settings.Default.MOmode)
            {
                return Path.Combine(ManageSettings.GetDataPath(), subpath);
            }

            //поиск по списку активных модов
            string ModsPath = ManageSettings.GetModsPath();
            foreach (var modName in GetModNamesListFromActiveMOProfile())
            {
                string possiblePath = Path.Combine(ModsPath, modName, subpath);
                if (IsDir)
                {
                    if (Directory.Exists(possiblePath))
                    {
                        return possiblePath;
                    }
                }
                else
                {
                    if (File.Exists(possiblePath))
                    {
                        return possiblePath;
                    }
                }
            }

            return pathInMods;
        }

        /// <summary>
        /// Gets setup.xml path from latest enabled mod like must be in Mod Organizer
        /// </summary>
        /// <returns></returns>
        public static string GetSetupXmlPathForCurrentProfile()
        {
            if (Properties.Settings.Default.MOmode)
            {
                string currentMOprofile = ManageSettings.GetMOSelectedProfileDirPath();

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile, "modlist.txt");

                    if (File.Exists(profilemodlistpath))
                    {
                        string[] lines = File.ReadAllLines(profilemodlistpath);

                        int linescount = lines.Length;
                        for (int i = 1; i < linescount; i++) // 1- означает пропуск нулевой строки, где комментарий
                        {
                            if (lines[i].StartsWith("+"))
                            {
                                string SetupXmlPath = Path.Combine(Properties.Settings.Default.ModsPath, lines[i].Remove(0, 1), "UserData", "setup.xml");
                                if (File.Exists(SetupXmlPath))
                                {
                                    return SetupXmlPath;
                                }
                            }
                        }
                    }
                }

                return Path.Combine(Properties.Settings.Default.OverwriteFolderLink, "UserData", "setup.xml");
            }
            else
            {
                return Path.Combine(Properties.Settings.Default.DataPath, "UserData", "setup.xml");
            }
        }

        public static string GetMetaParameterValue(string MetaFilePath, string NeededValue)
        {
            foreach (string line in File.ReadAllLines(MetaFilePath))
            {
                if (line.Length == 0)
                {
                }
                else
                {
                    if (line.StartsWith(NeededValue + "="))
                    {
                        return line.Remove(0, (NeededValue + "=").Length);
                    }
                }
            }

            return string.Empty;
        }

        public static string GetModFromModListContainsTheName(string name, bool OnlyFromEnabledMods = true)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string[] modList = ManageMO.GetModNamesListFromActiveMOProfile(OnlyFromEnabledMods);
                int nameLength = name.Length;
                int modListLength = modList.Length;
                for (int modlineNumber = 0; modlineNumber < modListLength; modlineNumber++)
                {
                    string modname = modList[modlineNumber];
                    if (modname.Length >= nameLength && ManageStrings.IsStringAContainsStringB(modname, name))
                    {
                        return modname;
                    }
                }
            }
            return string.Empty;
        }

        public static void MakeDummyFiles()
        {
            //Create dummy file and add hidden attribute
            if (!File.Exists(ManageSettings.GetDummyFilePath()))
            {
                File.WriteAllText(ManageSettings.GetDummyFilePath(), "dummy file need to execute mod organizer");
                ManageFilesFolders.HideFileFolder(ManageSettings.GetDummyFilePath(), true);
            }
        }

        public static string GetCategoryNameForTheIndex(string inputCategoryIndex, string[] categoriesList = null)
        {
            return GetCategoryIndexNameBase(inputCategoryIndex, categoriesList, true);
        }

        public static string GetCategoryIndexForTheName(string inputCategoryName, string[] categoriesList = null)
        {
            return GetCategoryIndexNameBase(inputCategoryName, categoriesList, false);
        }

        /// <summary>
        /// GetName = true means will be returned categorie name by index else will be returned index by name
        /// </summary>
        /// <param name="input"></param>
        /// <param name="categoriesList"></param>
        /// <param name="GetName"></param>
        /// <returns></returns>
        public static string GetCategoryIndexNameBase(string input, string[] categoriesList = null, bool GetName = true)
        {
            if (categoriesList == null)
            {
                categoriesList = File.ReadAllLines(ManageSettings.GetMOcategoriesPathForSelectedGame());
            }
            foreach (var category in categoriesList)
            {
                string[] categoryData = category.Split('|');
                if (categoryData[GetName ? 0 : 1] == input)
                {
                    return categoryData[GetName ? 1 : 0];
                }
            }

            return string.Empty;
        }

        public static string GetCategoriesForTheFolder(string moddir, string category, string[] categoriesList = null)
        {
            string Category = category;

            if (categoriesList == null)
            {
                categoriesList = File.ReadAllLines(ManageSettings.GetMOcategoriesPathForSelectedGame());
            }

            string[,] Categories =
            {
                { Path.Combine(moddir, "BepInEx", "Plugins"), ManageMO.GetCategoryIndexForTheName("Plugins",categoriesList), "dll" } //Plug-ins 51
                ,
                { Path.Combine(moddir, "UserData"), ManageMO.GetCategoryIndexForTheName("UserFiles",categoriesList), "*" } //UserFiles 53
                ,
                { Path.Combine(moddir, "UserData", "chara"), ManageMO.GetCategoryIndexForTheName("Characters",categoriesList), "png" } //Characters 54
                ,
                { Path.Combine(moddir, "UserData", "studio", "scene"), ManageMO.GetCategoryIndexForTheName("Studio scenes",categoriesList), "png"} //Studio scenes 57
                ,
                { Path.Combine(moddir, "Mods"), ManageMO.GetCategoryIndexForTheName("Sideloader",categoriesList), "zip" } //Sideloader 60
                ,
                { Path.Combine(moddir, "scripts"), ManageMO.GetCategoryIndexForTheName("ScriptLoader scripts",categoriesList), "cs"} //ScriptLoader scripts 86
                ,
                { Path.Combine(moddir, "UserData", "coordinate"), ManageMO.GetCategoryIndexForTheName("Coordinate",categoriesList), "png"} //Coordinate 87
                ,
                { Path.Combine(moddir, "UserData", "Overlays"), ManageMO.GetCategoryIndexForTheName("Overlay",categoriesList), "png"} //Overlay 88
                ,
                { Path.Combine(moddir, "UserData", "housing"), ManageMO.GetCategoryIndexForTheName("Housing",categoriesList), "png"} //Housing 89
                ,
                { Path.Combine(moddir, "UserData", "housing"), ManageMO.GetCategoryIndexForTheName("Cardframe",categoriesList), "png"} //Cardframe 90
            };

            int CategoriesLength = Categories.Length / 3;
            for (int i = 0; i < CategoriesLength; i++)
            {
                string dir = Categories[i, 0];
                string categorieNum = Categories[i, 1];
                string extension = Categories[i, 2];
                if (
                    (
                        (category.Length > 0
                        && !category.Contains("," + categorieNum)
                        && !category.Contains(categorieNum + ",")
                        )
                     || category.Length == 0
                    )
                    && Directory.Exists(dir)
                    && !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(dir)
                    && ManageFilesFolders.IsAnyFileExistsInTheDir(dir, extension)
                   )
                {
                    if (Category.Length > 0)
                    {
                        if (Category.Substring(Category.Length - 1, 1) == ",")
                        {
                            Category += categorieNum;
                        }
                        else
                        {
                            Category += "," + categorieNum;
                        }
                    }
                    else
                    {
                        Category = categorieNum + ",";
                    }
                }
            }

            return Category;
        }
    }
}
