using AI_Helper.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AI_Helper.Manage
{
    class MOManage
    {
        public static void SetModOrganizerINISettingsForTheGame()
        {
            RedefineCategoriesDat();

            RedefineModOrganizerIni();

            IniFile INI = new IniFile(SettingsManage.GetModOrganizerINIpath());

            SetCommonIniValues(INI);
            SetCustomExecutablesIniValues(INI);
        }

        private static void SetCustomExecutablesIniValues(IniFile INI)
        {
            string[,] IniValues =
                {
                    //customExecutables
                    {
                        SettingsManage.GetCurrentGameEXEName()
                        ,
                        @"1\title"
                    }
                ,
                    {
                        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
                        ,
                        @"1\binary"
                    }
                ,
                    {
                        SettingsManage.GetDataPath()
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
                        SettingsManage.GetINISettingsEXEName()
                        ,
                        @"2\title"
                    }
                ,
                    {
                        SettingsManage.GetSettingsEXEPath()
                        ,
                        @"2\binary"
                    }
                ,
                    {
                        SettingsManage.GetDataPath()
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
                        Path.Combine(SettingsManage.GetStudioEXEName())
                        ,
                        @"3\title"
                    }
                ,
                    {
                        Path.Combine(SettingsManage.GetDataPath(),SettingsManage.GetStudioEXEName()+".exe")
                        ,
                        @"3\binary"
                    }
                ,
                    {
                        SettingsManage.GetDataPath()
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
            int IniValuesLength = IniValues.Length / 2;
            for (int i = 0; i < IniValuesLength; i++)
            {
                if (!IniValuesDict.ContainsKey(IniValues[i, 1]))
                {
                    IniValuesDict.Add(IniValues[i, 1], IniValues[i, 0]);
                }
            }

            int ExecutablesCount;
            try
            {
                ExecutablesCount = int.Parse(IniValues[IniValuesLength - 1, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0]);
            }
            catch
            {
                return;//если не число, выйти
            }

            var CurrentGame = SettingsManage.GetListOfExistsGames()[Properties.Settings.Default.CurrentGameListIndex];
            if (CurrentGame.GetGameEXENameX32().Length > 0)
            {
                ExecutablesCount++;
                IniValuesDict.Add(ExecutablesCount + @"\title", CurrentGame.GetGameEXENameX32());
                IniValuesDict.Add(ExecutablesCount + @"\binary", Path.Combine(SettingsManage.GetDataPath(), CurrentGame.GetGameEXENameX32() + ".exe"));
                IniValuesDict.Add(ExecutablesCount + @"\workingDirectory", SettingsManage.GetDataPath());
            }
            if (CurrentGame.GetGameStudioEXENameX32().Length > 0)
            {
                ExecutablesCount++;
                IniValuesDict.Add(ExecutablesCount + @"\title", CurrentGame.GetGameStudioEXENameX32());
                IniValuesDict.Add(ExecutablesCount + @"\binary", Path.Combine(SettingsManage.GetDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe"));
                IniValuesDict.Add(ExecutablesCount + @"\workingDirectory", SettingsManage.GetDataPath());
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

                    INI.WriteINI("customExecutables", key, IniValue);
                }
                cnt++;
            }

            INI.WriteINI("customExecutables", "size", ExecutablesCount.ToString());
        }

        private static void SetCommonIniValues(IniFile INI)
        {
            string[,] IniValues =
                {
                    //General
                    {
                        SettingsManage.GetCurrentGamePath()
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
                        SettingsManage.GetModsPath()
                        ,
                        "Settings"
                        ,
                        @"mod_directory"
                    }
                ,
                    {
                        Path.Combine(SettingsManage.GetCurrentGamePath(), "Downloads")
                        ,
                        "Settings"
                        ,
                        @"download_directory"
                    }
                ,
                    {
                        Path.Combine(SettingsManage.GetCurrentGamePath(), "MO", "profiles")
                        ,
                        "Settings"
                        ,
                        @"profiles_directory"
                    }
                ,
                    {
                        Path.Combine(SettingsManage.GetCurrentGamePath(), "MO", "overwrite")
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
                INI.WriteINI(IniValues[i, 1], IniValues[i, 2], IniValue);
            }
        }

        private static string GetSelectedProfileName()
        {
            string ret = INIManage.GetINIValueIfExist(Path.Combine(SettingsManage.GetCurrentGamePath(), "MO", "ModOrganizer.ini"), "selected_profile", "General");
            return ret.Length > 0 ? ret : SettingsManage.GetCurrentGameFolderName();
        }

        private static void RedefineModOrganizerIni()
        {
            string[] categoriesdatGameAndLocalPaths = new string[2]
            {
                Path.Combine(SettingsManage.GetCurrentGamePath(),"MO", "ModOrganizer.ini")
                ,
                Path.Combine(SettingsManage.GetMOdirPath(), "ModOrganizer.ini")
            };

            var moINIinfo = new FileInfo(categoriesdatGameAndLocalPaths[1]);
            if (!SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLink(moINIinfo) || !SymbolicLinkSupport.FileInfoExtensions.IsSymbolicLinkValid(moINIinfo))
            {
                File.Delete(moINIinfo.FullName);
            }

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
                FileFolderOperations.Symlink
                  (
                   categoriesdatGameAndLocalPaths[0]
                   ,
                   categoriesdatGameAndLocalPaths[1]
                   ,
                   true
                  );
            }
        }

        public static void RedefineCategoriesDat()
        {
            string[] categoriesdatGameAndLocalPaths = new string[2]
            {
                Path.Combine(SettingsManage.GetCurrentGamePath(),"MO", "categories.dat")
                ,
                Path.Combine(SettingsManage.GetMOdirPath(), "categories.dat")
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
                FileFolderOperations.Symlink
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
                string currentMOprofile = INIManage.GetINIValueIfExist(Path.Combine(Properties.Settings.Default.MODirPath, "ModOrganizer.ini"), "selected_profile", "General");

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(Properties.Settings.Default.MODirPath, "profiles", currentMOprofile, "modlist.txt");

                    INIManage.InsertLineInFile(profilemodlistpath, (Activate ? "+" : "-") + modname, 1, modAfterWhichInsert, PlaceAfter);
                }
            }
        }

        /// <summary>
        /// Writes required parameters in meta.ini
        /// </summary>
        /// <param name="moddir"></param>
        /// <param name="category"></param>
        /// <param name="version"></param>
        /// <param name="comments"></param>
        /// <param name="notes"></param>
        public static void WriteMetaINI(string moddir, string category = "", string version = "", string comments = "", string notes = "")
        {
            if (Directory.Exists(moddir))
            {
                string metaPath = Path.Combine(moddir, "meta.ini");
                IniFile INI = new IniFile(metaPath);

                bool IsKeyExists = INI.KeyExists("category", "General");
                if (!IsKeyExists || (IsKeyExists && category.Length > 0 && INI.ReadINI("General", "category").Replace("\"", string.Empty).Length == 0))
                {
                    INI.WriteINI("General", "category", "\"" + category + "\"");
                }

                if (version.Length > 0)
                {
                    INI.WriteINI("General", "version", version);
                }

                INI.WriteINI("General", "gameName", SettingsManage.GETMOCurrentGameName());

                if (comments.Length > 0)
                {
                    INI.WriteINI("General", "comments", comments);
                }

                if (notes.Length > 0)
                {
                    INI.WriteINI("General", "notes", "\"" + notes + "\"");
                }

                INI.WriteINI("General", "validated", "true");
            }
        }

        public static string[] GetModsListFromActiveMOProfile(bool OnlyEnabled = true)
        {
            string currentMOprofile = INIManage.GetINIValueIfExist(Path.Combine(Properties.Settings.Default.MODirPath, "ModOrganizer.ini"), "selected_profile", "General");

            if (currentMOprofile.Length > 0)
            {
                string profilemodlistpath = Path.Combine(Properties.Settings.Default.MODirPath, "profiles", currentMOprofile, "modlist.txt");

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

        public static string GetOverwriteFolderLocation()
        {
            string IniValue = INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, "overwrite_directory", "Settings");

            if (IniValue.Length > 0)
            {
                return IniValue;
            }
            else
            {
                return Path.Combine(Properties.Settings.Default.MODirPath, "overwrite");
            }

        }

        /// <summary>
        /// Gets setup.xml path from latest enabled mod like must be in Mod Organizer
        /// </summary>
        /// <returns></returns>
        public static string GetSetupXmlPathForCurrentProfile()
        {
            if (Properties.Settings.Default.MOmode)
            {
                string currentMOprofile = INIManage.GetINIValueIfExist(SettingsManage.GetModOrganizerINIpath(), "selected_profile", "General");

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(SettingsManage.GetMOdirPath(), "profiles", currentMOprofile, "modlist.txt");

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
                string[] modList = MOManage.GetModsListFromActiveMOProfile(OnlyFromEnabledMods);
                int nameLength = name.Length;
                int modListLength = modList.Length;
                for (int modlineNumber = 0; modlineNumber < modListLength; modlineNumber++)
                {
                    string modname = modList[modlineNumber];
                    if (modname.Length >= nameLength && StringEx.IsStringAContainsStringB(modname, name))
                    {
                        return modname;
                    }
                }
            }
            return string.Empty;
        }
    }
}
