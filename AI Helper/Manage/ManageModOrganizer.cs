using AIHelper.Games;
using AIHelper.SharedData;
using CheckForEmptyDir;
using INIFileMan;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static AIHelper.Manage.ManageModOrganizer;
using static AIHelper.Manage.ManageModOrganizerMods;

namespace AIHelper.Manage
{
    static class ManageModOrganizer
    {

        internal static string MOremoveByteArray(string mOSelectedProfileDirPath)
        {
            if (mOSelectedProfileDirPath.StartsWith("@ByteArray(", StringComparison.InvariantCulture))
            {
                return mOSelectedProfileDirPath
                    .Remove(mOSelectedProfileDirPath.Length - 1, 1)
                    .Replace("@ByteArray(", string.Empty);
            }
            else
            {
                return mOSelectedProfileDirPath;
            }
        }

        public static void RedefineGameMoData()
        {
            //MOini
            ManageSettings.GetMOiniPath().ReCreateFileLinkWhenNotValid(ManageSettings.GetMOiniPathForSelectedGame(), true);
            //Categories
            ManageSettings.GetMOcategoriesPath().ReCreateFileLinkWhenNotValid(ManageSettings.GetMOcategoriesPathForSelectedGame(), true);
        }

        internal static string GetMoVersion()
        {
            var exeversion = System.Diagnostics.FileVersionInfo.GetVersionInfo(ManageSettings.GetMOexePath());
            return exeversion.FileVersion;
        }

        public static void SetModOrganizerIniSettingsForTheGame()
        {
            RedefineGameMoData();

            //менять настройки МО только когда игра меняется
            if (!Properties.Settings.Default.CurrentGameIsChanging && Path.GetDirectoryName(ManageSettings.GetMOdirPath()) != Properties.Settings.Default.ApplicationStartupPath)
            {
                return;
            }

            INIFile ini = ManageIni.GetINIFile(ManageSettings.GetModOrganizerIniPath());

            SetCommonIniValues(ini);

            //await Task.Run(() => SetCustomExecutablesIniValues(INI)).ConfigureAwait(true);
            FixCustomExecutablesIniValues();
            Properties.Settings.Default.SetModOrganizerINISettingsForTheGame = false;

            //Properties.Settings.Default.CurrentGameIsChanging = false;
        }

        /// <summary>
        /// returns filename
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        internal static string MakeMoProfileModlistFileBuckup(string suffix = "")
        {
            var modlistPath = ManageSettings.GetCurrentMoProfileModlistPath();
            var lastBackup = GetLastBak(suffix);
            if (lastBackup == null || (File.Exists(modlistPath) && File.Exists(lastBackup) && File.ReadAllText(lastBackup) != File.ReadAllText(modlistPath)))
            {
                var targetFile = modlistPath + "." + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture) + suffix;
                File.Copy(modlistPath, targetFile);
                return targetFile;
            }
            return string.Empty;
        }

        /// <summary>
        /// get last modlist buckup path
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        private static string GetLastBak(string suffix)
        {
            FileInfo last = null;
            foreach (var file in new DirectoryInfo(ManageSettings.GetMoSelectedProfileDirPath()).EnumerateFiles("modlist.txt.*" + suffix))
            {
                if (last == null || last.LastWriteTime < file.LastWriteTime)
                {
                    last = file;
                }
            }
            return last?.FullName;
        }

        /// <summary>
        /// changes incorrect relative paths "../Data" fo custom executable for absolute Data path of currents game
        /// </summary>
        /// <param name="ini"></param>
        internal static void FixCustomExecutablesIniValues(INIFile ini = null)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.GetModOrganizerIniPath());
            }

            var customExecutables = new CustomExecutables(ini);
            var customsToRemove = new List<string>();
            foreach (var record in customExecutables.List)
            {
                if (string.IsNullOrWhiteSpace(record.Value.Binary))
                {
                    customsToRemove.Add(record.Key); // add invalid custom to remove list
                    continue;
                }

                foreach (var attribute in new[] { "binary", "workingDirectory" })
                {
                    try
                    {
                        string fullPath = "";
                        if (record.Value.Attribute[attribute].Length > 0)
                        {
                            fullPath = Path.GetFullPath(record.Value.Attribute[attribute]);
                        }
                        bool isFile = attribute == "binary";
                        if ((isFile && File.Exists(fullPath)) || (!isFile && Directory.Exists(fullPath)))
                        {
                            //not need to change if path is exists
                        }
                        else
                        {
                            if (record.Value.Attribute[attribute].StartsWith("..", StringComparison.InvariantCulture))
                            {
                                //suppose relative path was from MO dir ..\%MODir%
                                //replace .. to absolute path of current game directory
                                var targetcorrectedrelative = record.Value.Attribute[attribute]
                                        .Remove(0, 2).Insert(0, ManageSettings.GetCurrentGamePath());

                                //replace other slashes
                                var targetcorrectedabsolute = Path.GetFullPath(targetcorrectedrelative);

                                FixExplorerPlusPlusPath(ref targetcorrectedabsolute);

                                record.Value.Attribute[attribute] = CustomExecutables.NormalizePath(targetcorrectedabsolute);
                            }
                            else
                            {
                                var newPath = record.Value.Attribute[attribute].Replace("/", "\\");
                                if (FixExplorerPlusPlusPath(ref newPath))
                                {
                                    record.Value.Attribute[attribute] = CustomExecutables.NormalizePath(newPath);
                                }
                            }
                        }
                    }
                    catch
                    {
                        ManageLogs.Log("FixCustomExecutablesIniValues:Error while path fix.\r\nKey=" + record.Key + "\r\nPath=" + record.Value.Binary);
                    }
                }
            }

            //remove invalid customs
            foreach (var custom in customsToRemove)
            {
                customExecutables.List.Remove(custom);
            }

            customExecutables.Save();
        }

        /// <summary>
        /// check if the <paramref name="game"/> is valid to use with the app
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool IsValidGame(this Game game)
        {
            return game != null
                   &&
                   Directory.Exists(game.GetGamePath())
                   &&
                   game.HasMainGameExe() // has main exe to play
                   &&
                   game.HasModOrganizerIni() // has mo ini
                   &&
                   game.HasAnyWorkProfile() // has atleast one profile
                   &&
                   game.CheckCategoriesDat()
                   ;
        }

        /// <summary>
        /// check if the <paramref name="game"/> has ModOrganizer.ini file in MO folder
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool HasModOrganizerIni(this Game game)
        {
            return File.Exists(Path.Combine(game.GetGamePath(), "MO", "ModOrganizer.ini"));
        }

        /// <summary>
        /// will create categories.dat file in mo folder of the <paramref name="game"/> if missing
        /// </summary>
        /// <param name="game"></param>
        internal static bool CheckCategoriesDat(this Game game)
        {
            string categories;
            if (!File.Exists(categories = Path.Combine(game.GetGamePath(), "MO", "categories.dat")))
            {
                Directory.CreateDirectory(Path.Combine(game.GetGamePath(), "MO"));

                File.WriteAllText(categories, string.Empty);
            }

            return true;
        }

        /// <summary>
        /// check if the <paramref name="game"/> has main execute file
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool HasMainGameExe(this Game game)
        {
            return File.Exists(Path.Combine(ManageSettings.GetGamesFolderPath(), game.GetDataPath(), game.GetGameExeName() + ".exe"));
        }

        /// <summary>
        /// check if MO dir of input <paramref name="game"/> has any profile with modlist.txt
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool HasAnyWorkProfile(this Game game)
        {
            foreach (var profileDir in Directory.EnumerateDirectories(Path.Combine(game.GetGamePath(), "MO", "Profiles")))
            {
                if (File.Exists(Path.Combine(profileDir, "modlist.txt")))
                {
                    return true;
                }
            }
            return false;
            //!Path.Combine(game.GetGamePath(), "MO", "Profiles").IsNullOrEmptyDirectory(mask: "modlist.txt", recursive: true, preciseMask: true) // modlist.txt must exist atleast one
        }

        /// <summary>
        /// fixes alternative path for explorer++ to path to native mo dir
        /// </summary>
        /// <param name="inputPath"></param>
        private static bool FixExplorerPlusPlusPath(ref string inputPath)
        {
            var altModOrganizerDirNameForEpp = Regex.Match(inputPath, @"\\([^\\]+)\\explorer\+\+");
            if (altModOrganizerDirNameForEpp.Success && altModOrganizerDirNameForEpp.Result("$1").StartsWith("MO", StringComparison.InvariantCultureIgnoreCase))
            {
                inputPath = inputPath
                    .Remove(altModOrganizerDirNameForEpp.Index, altModOrganizerDirNameForEpp.Length)
                    .Insert(altModOrganizerDirNameForEpp.Index, @"\..\..\MO\explorer++");

                inputPath = Path.GetFullPath(inputPath);

                return true;
            }

            return false;
        }

        internal class CustomExecutables
        {
            /// <summary>
            /// list of custom executables
            /// </summary>
            internal Dictionary<string, CustomExecutable> List;
            INIFile _ini;

            /// <summary>
            /// Init new custom executables and load from default ModOrganizer.ini path for the selected game.
            /// </summary>
            internal CustomExecutables()
            {
                LoadFrom(ManageIni.GetINIFile(ManageSettings.GetMOiniPath()));
            }

            /// <summary>
            /// Init new custom executables and load from specified ini.
            /// </summary>
            /// <param name="ini"></param>
            internal CustomExecutables(INIFile ini)
            {
                LoadFrom(ini);
            }

            private int _loadedListCustomsCount;

            bool NeedSetCurrentProfileSettingsIniSet = true;
            INIFile settingsIni;

            /// <summary>
            /// load customs in list
            /// </summary>
            /// <param name="ini"></param>
            internal void LoadFrom(INIFile ini)
            {
                if (List != null && List.Count > 0) // already loaded
                {
                    return;
                }

                ini.Configuration.AssigmentSpacer = ""; // no spaces for mo ini

                this._ini = ini; // set ini reference

                List = new Dictionary<string, CustomExecutable>();
                foreach (var keyData in ini.GetSectionKeyValuePairs("customExecutables"))
                {
                    var numName = keyData.Key.Split('\\');//numName[0] - number of customexecutable , numName[0] - name of attribute
                    if (numName.Length != 2)
                    {
                        continue;
                    }
                    var customNumber = numName[0];
                    var customKeyName = numName[1];

                    if (!List.ContainsKey(customNumber))
                    {
                        List.Add(customNumber, new CustomExecutable());
                    }

                    List[customNumber].Attribute[customKeyName] = keyData.Value;
                }

                _loadedListCustomsCount = List.Count;

                //Normalize values
                foreach (var customExecutable in List)
                {
                    foreach (var attributeKey in customExecutable.Value.Attribute.Keys.ToList())
                    {
                        ApplyNormalize(customExecutable.Value, attributeKey);
                    }

                    if (NeedSetCurrentProfileSettingsIniSet)
                    {
                        NeedSetCurrentProfileSettingsIniSet = false;

                        settingsIni = ManageIni.GetINIFile(ManageSettings.GetMoSelectedProfileSettingsPath());
                    }
                    if (!NeedSetCurrentProfileSettingsIniSet)
                    {
                        if (settingsIni.KeyExists(customExecutable.Value.Title, "custom_overwrites"))
                        {
                            customExecutable.Value.MoTargetMod = settingsIni.GetKey("custom_overwrites", customExecutable.Value.Title);
                        }
                    }
                }

            }

            /// <summary>
            /// keys frol custom executables list to remove while Save execution
            /// </summary>
            internal HashSet<string> listToRemoveKeys = new HashSet<string>();

            /// <summary>
            /// Save ini with changed customs
            /// </summary>
            internal void Save()
            {
                if (_ini == null)
                {
                    return;
                }

                bool changed = false;
                bool sectionCleared = _loadedListCustomsCount != List.Count || listToRemoveKeys.Count > 0;
                if (sectionCleared)
                {
                    changed = true;
                    _ini.ClearSection("customExecutables");
                }

                int customExecutableNumber = 0; // use new executable number when section was cleared and need to renumber executable numbers
                foreach (var customExecutable in List)
                {
                    if (sectionCleared)
                    {
                        if (listToRemoveKeys.Contains(customExecutable.Key)) // skip custom exe to be saved
                        {
                            changed = true;
                            continue;
                        }

                        customExecutableNumber++;
                    }

                    foreach (var attributeKey in customExecutable.Value.Attribute.Keys.ToList())
                    {
                        string keyName = (sectionCleared ? customExecutableNumber + "" : customExecutable.Key) + "\\" + attributeKey;

                        // normalize values
                        var valueBeforeNormalize = customExecutable.Value.Attribute[attributeKey];
                        ApplyNormalize(customExecutable.Value, attributeKey);

                        if (sectionCleared || customExecutable.Value.Attribute[attributeKey] != valueBeforeNormalize || !_ini.KeyExists(keyName, "customExecutables") || _ini.GetKey("customExecutables", keyName) != customExecutable.Value.Attribute[attributeKey]) // write only if not equal
                        {
                            changed = true;
                            _ini.SetKey("customExecutables", keyName, customExecutable.Value.Attribute[attributeKey]);
                        }
                    }

                    // set target mod if exists
                    if (!string.IsNullOrWhiteSpace(customExecutable.Value.MoTargetMod) && settingsIni.GetKey(customExecutable.Value.Title, "custom_overwrites") != customExecutable.Value.MoTargetMod)
                    {
                        settingsIni.SetKey("custom_overwrites", customExecutable.Value.Title, customExecutable.Value.MoTargetMod);
                    }
                }

                if (changed)
                {
                    _ini.SetKey("customExecutables", "size", (sectionCleared ? customExecutableNumber : List.Count) + "");
                }
            }

            /// <summary>
            /// apply normalization of values
            /// </summary>
            /// <param name="customExecutable"></param>
            /// <param name="attributeKey"></param>
            private static void ApplyNormalize(CustomExecutable customExecutable, string attributeKey)
            {
                switch (attributeKey)
                {
                    case "binary":
                    case "workingDirectory":
                        customExecutable.Attribute[attributeKey] = NormalizePath(customExecutable.Attribute[attributeKey]);
                        break;
                    case "arguments":
                        customExecutable.Attribute[attributeKey] = NormalizeArguments(customExecutable.Attribute[attributeKey]);
                        break;
                    case "toolbar":
                    case "ownicon":
                    case "hide":
                        customExecutable.Attribute[attributeKey] = NormalizeBool(customExecutable.Attribute[attributeKey]);
                        break;
                }
            }

            /// <summary>
            /// normalize boolean for mod organizer ini custom executable
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            protected static string NormalizeBool(string value)
            {
                if (!string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    //false if value not one of false or true
                    return "false";
                }

                return value.Trim();
            }

            /// <summary>
            /// normalize path for mod organizer ini custom executable
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            internal static string NormalizePath(string value)
            {
                return value.Replace('\\', '/');
            }

            /// <summary>
            /// normalize argumants for mod organizer ini custom executable
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            protected static string NormalizeArguments(string value)
            {
                value = value.Trim();

                if (string.IsNullOrEmpty(value) || value == "\\\"\\\"")
                {
                    return value;
                }

                int startIndex = 0;
                int endIndex = value.Length - 1;

                bool IsBackSlashOrQuote = false;
                for (int i = endIndex; i >= startIndex; i--)
                {
                    if (value[i] == '\\' || (!IsBackSlashOrQuote && value[i] == '\"'))
                    {
                        IsBackSlashOrQuote = !IsBackSlashOrQuote;

                        if (i == 0 && value[i] == '\"')
                        {
                            value = value.Insert(i, "\\");
                        }
                    }
                    else
                    {
                        if (IsBackSlashOrQuote)
                        {
                            value = value.Insert(i + 1, "\\");
                            IsBackSlashOrQuote = !IsBackSlashOrQuote;
                        }
                    }
                }

                var startsQuoted = value.StartsWith("\\\"", StringComparison.InvariantCulture);
                var endsQuoted = value.EndsWith("\\\"", StringComparison.InvariantCulture);

                return (startsQuoted ? "" : "\\\"") + value + (endsQuoted ? "" : "\\\"");
            }

            internal class CustomExecutable
            {
                /// <summary>
                /// target mod for mod organizer new created files of the exe file
                /// </summary>
                internal string MoTargetMod = "";

                /// <summary>
                /// <list type="s">
                ///     <listheader>
                ///         <description>List of possible attributes:</description>
                ///     </listheader>
                ///     <item>
                ///         <term>title</term>
                ///         <description>(Required!) title of custom executable</description>
                ///     </item>
                ///     <item>
                ///         <term>binary</term>
                ///         <description>(Required!) path to the exe</description>
                ///     </item>
                ///     <item>
                ///         <term>workingDirectory</term>
                ///         <description>(Optional) Working directory. By defaule willbe directory where is binary located.</description>
                ///     </item>
                ///     <item>
                ///         <term>arguments</term>
                ///         <description>(Optional) Arguments for binary. Empty by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>toolbar</term>
                ///         <description>(Optional) Enable toolbar. False by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>ownicon</term>
                ///         <description>(Optional) Use own icon for the exe, else will be icon of MO. False by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>hide</term>
                ///         <description>(Optional) Hide the custom exe? False by default.</description>
                ///     </item>
                ///     <item>
                ///         <term>steamAppID</term>
                ///         <description>(Optional) Steam app id for binary. Empty by default.</description>
                ///     </item>
                /// </list>
                /// </summary>
                /// <example>
                ///     example of use:
                ///     <code>
                ///         customExecutable.attribute["title"] = "New custom exe"
                ///     </code>
                /// </example>
                internal Dictionary<string, string> Attribute = new Dictionary<string, string>()
                {
                    { "title" , null},
                    { "binary" , null},
                    { "workingDirectory" , string.Empty},
                    { "arguments" , string.Empty},
                    { "toolbar" , "false"},
                    { "ownicon" , "false"},
                    { "hide" , "false"},
                    { "steamAppID" , string.Empty},
                };


#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1822 // Mark members as static
                /// <summary>
                /// (Required!) title of custom executable
                /// </summary>
                internal string Title { get => Attribute["title"]; set => Attribute["title"] = value; }

                /// <summary>
                /// (Required!) path to the exe
                /// </summary>
                internal string Binary { get => Attribute["binary"]; set => Attribute["binary"] = NormalizePath(value); }

                /// <summary>
                /// (Optional) Working directory. By defaule willbe directory where is binary located.
                /// </summary>
                internal string WorkingDirectory { get => Attribute["workingDirectory"]; set => Attribute["workingDirectory"] = NormalizePath(value); }

                /// <summary>
                /// (Optional) Arguments for binary. Empty by default.
                /// </summary>
                internal string Arguments { get => Attribute["arguments"]; set => Attribute["arguments"] = NormalizeArguments(value); }

                /// <summary>
                /// (Optional) Enable toolbar. False by default.
                /// </summary>
                internal string Toolbar { get => Attribute["toolbar"]; set => Attribute["toolbar"] = NormalizeBool(value); }

                /// <summary>
                /// (Optional) Use own icon for the exe, else will be icon of MO. False by default.
                /// </summary>
                internal string Ownicon { get => Attribute["ownicon"]; set => Attribute["ownicon"] = NormalizeBool(value); }

                /// <summary>
                /// (Optional) Hide the custom exe? False by default.
                /// </summary>
                internal string Hide { get => Attribute["hide"]; set => Attribute["hide"] = NormalizeBool(value); }

                /// <summary>
                /// (Optional) Steam app id for binary. Empty by default.
                /// </summary>
                internal string SteamAppId { get => Attribute["steamAppID"]; set => Attribute["steamAppID"] = value; }

#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE1006 // Naming Styles
            }
        }

        //private static void SetCustomExecutablesIniValues(INIFile INI)
        //{
        //    if (Properties.Settings.Default.SetModOrganizerINISettingsForTheGame)
        //    {
        //        return;
        //    }

        //    Properties.Settings.Default.SetModOrganizerINISettingsForTheGame = true;

        //    string[,] IniValues = new string[,] { };

        //    if (!ManageSettings.MOIsNew)
        //    {
        //        IniValues = new string[,]
        //            {
        //            //customExecutables
        //            {
        //                ManageSettings.GetCurrentGameEXEName()
        //                ,
        //                @"1\title"
        //            }
        //        ,
        //            {
        //                Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameEXEName() + ".exe")
        //                ,
        //                @"1\binary"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetCurrentGameDataPath()
        //                ,
        //                @"1\workingDirectory"
        //            }
        //        ,
        //            {
        //                "true"
        //                ,
        //                @"1\ownicon"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetINISettingsEXEName()
        //                ,
        //                @"2\title"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetSettingsEXEPath()
        //                ,
        //                @"2\binary"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetCurrentGameDataPath()
        //                ,
        //                @"2\workingDirectory"
        //            }
        //        ,
        //            {
        //                "true"
        //                ,
        //                @"2\ownicon"
        //            }
        //        //,
        //        //    {
        //        //        "Explore Virtual Folder"
        //        //        ,
        //        //        @"3\title"
        //        //    }
        //        //,
        //        //    {
        //        //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++", "Explorer++.exe")
        //        //        ,
        //        //        @"3\binary"
        //        //    }
        //        //,
        //        //    {
        //        //        SettingsManage.GetDataPath()
        //        //        ,
        //        //        @"3\arguments"
        //        //    }
        //        //,
        //        //    {
        //        //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++")
        //        //        ,
        //        //        @"3\workingDirectory"
        //        //    }
        //        //,
        //        //    {
        //        //        "true"
        //        //        ,
        //        //        @"3\ownicon"
        //        //    }
        //        //,
        //        //    {
        //        //        "Skyrim"
        //        //        ,
        //        //        @"4\title"
        //        //    }
        //        //,
        //        //    {
        //        //        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
        //        //        ,
        //        //        @"4\binary"
        //        //    }
        //        //,
        //        //    {
        //        //        SettingsManage.GetDataPath()
        //        //        ,
        //        //        @"4\workingDirectory"
        //        //    }
        //        ,
        //            {
        //                Path.Combine(ManageSettings.GetStudioEXEName())
        //                ,
        //                @"3\title"
        //            }
        //        ,
        //            {
        //                Path.Combine(ManageSettings.GetCurrentGameDataPath(),ManageSettings.GetStudioEXEName()+".exe")
        //                ,
        //                @"3\binary"
        //            }
        //        ,
        //            {
        //                ManageSettings.GetCurrentGameDataPath()
        //                ,
        //                @"3\workingDirectory"
        //            }
        //        ,
        //            {
        //                "true"
        //                ,
        //                @"3\ownicon"
        //            }
        //        };
        //    }


        //    Dictionary<string, string> IniValuesDict = new Dictionary<string, string>();

        //    string[,] iniParameters =
        //        {
        //            {
        //               "title"
        //               ,
        //               "rs"
        //            }
        //        ,
        //            {
        //                "binary"
        //            ,
        //                "rs"
        //            }
        //        ,
        //            {
        //                "workingDirectory"
        //            ,
        //                "rs"
        //            }
        //        ,
        //            {
        //                "arguments"
        //            ,
        //                "s"
        //            }
        //        ,
        //            {
        //                "toolbar"
        //            ,
        //                "b"
        //            }
        //        ,
        //            {
        //                "ownicon"
        //            ,
        //                "b"
        //            }
        //        ,
        //            {
        //                "steamAppID"
        //            ,
        //                "s"
        //            }
        //        ,
        //            {
        //                "hide"
        //            ,
        //                "b"
        //            }
        //    };

        //    int resultindex = 0;//конечное количество добавленных exe
        //    if (!ManageSettings.MOIsNew)
        //    {
        //        //заполнить словарь значениями массива строк
        //        resultindex = 1;//конечное количество добавленных exe
        //        int IniValuesLength = IniValues.Length / 2;//длина массива, деленая на 2 т.к. каждый элемент состоит из двух
        //        int exclude = 0;//будет равен индексу, который надо пропустить
        //        int skippedCnt = 0;//счет количества пропущенных, нужно для правильного подсчета resultindex
        //        for (int i = 0; i < IniValuesLength; i++)
        //        {
        //            var curindex = int.Parse(IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0], CultureInfo.InvariantCulture);
        //            var parameterName = IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[1];

        //            if (curindex == exclude)
        //            {
        //                continue;
        //            }
        //            else if (exclude != 0)
        //            {
        //                exclude = 0;
        //            }
        //            //Если название пустое, то пропускать все значения с таким индексом
        //            if (parameterName == "title" && IniValues[i, 0].Length == 0)
        //            {
        //                exclude = curindex;
        //                skippedCnt++;
        //                continue;
        //            }

        //            if (curindex - skippedCnt > resultindex && exclude == 0)
        //            {
        //                resultindex++;
        //            }

        //            var ResultparameterName = resultindex + @"\" + parameterName;

        //            if (!IniValuesDict.ContainsKey(ResultparameterName))
        //            {
        //                IniValuesDict.Add(ResultparameterName, IniValues[i, 0]);
        //            }
        //        }
        //    }

        //    var ExecutablesCount = resultindex;
        //    //try
        //    //{
        //    //    ExecutablesCount = int.Parse(IniValues[IniValuesLength - 1, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0]);
        //    //}
        //    //catch
        //    //{
        //    //    return;//если не число, выйти
        //    //}

        //    AddCustomExecutables(INI, out Dictionary<string, string> customExecutables, ref ExecutablesCount);

        //    if (customExecutables.Count > 0)
        //    {
        //        //очистка секции
        //        INI.ClearSection("customExecutables", false);
        //        foreach (var pair in customExecutables)
        //        {
        //            if (!IniValuesDict.ContainsKey(pair.Key))
        //            {
        //                IniValuesDict.Add(pair.Key, pair.Value);
        //            }
        //        }

        //        int cnt = 1;
        //        int iniParametersLength = iniParameters.Length / 2;
        //        while (cnt <= ExecutablesCount)
        //        {
        //            for (int i = 0; i < iniParametersLength; i++)
        //            {
        //                string key = cnt + @"\" + iniParameters[i, 0];
        //                string IniValue;
        //                bool keyExists = IniValuesDict.ContainsKey(key);
        //                string subquote = key.EndsWith(@"\arguments", StringComparison.InvariantCulture) && keyExists ? "\\\"" : string.Empty;
        //                if (keyExists)
        //                {
        //                    IniValue = subquote + IniValuesDict[key].Replace(@"\", key.EndsWith(@"\arguments", StringComparison.InvariantCulture) ? @"\\" : "/") + subquote;
        //                }
        //                else
        //                {
        //                    IniValue = subquote + (iniParameters[i, 1].EndsWith("b", StringComparison.InvariantCulture) ? "false" : string.Empty) + subquote;
        //                }

        //                INI.WriteINI("customExecutables", key, IniValue, false);
        //            }
        //            cnt++;
        //        }

        //        //Hardcoded exe Game exe and Explorer++
        //        //ExecutablesCount += 2;

        //        INI.WriteINI("customExecutables", "size", ExecutablesCount.ToString(CultureInfo.InvariantCulture));
        //    }
        //    else
        //    {
        //        INI.SaveINI(true, true);
        //    }

        //    Properties.Settings.Default.SetModOrganizerINISettingsForTheGame = false;
        //}

        /// <summary>
        /// true if MO have base plugin
        /// </summary>
        /// <returns></returns>
        internal static bool IsMo23OrNever()
        {
            var ver = GetMoVersion();
            return ver != null && ver.Length > 2 && ver[0] == '2' && int.Parse(ver[2].ToString(), CultureInfo.InvariantCulture) > 2;
        }

        //private static void AddCustomExecutables(INIFile INI, out Dictionary<string, string> customExecutables, ref int ExecutablesCount)
        //{
        //    customExecutables = new Dictionary<string, string>();

        //    var exists = INI.ReadSectionValuesToArray("customExecutables");

        //    var exeEqual = 0;
        //    var OneexeNotEqual = false;
        //    var executablesCount = ExecutablesCount;
        //    Dictionary<string, string> customs = new Dictionary<string, string>();

        //    if (!ManageSettings.MOIsNew)
        //    {
        //        var CurrentGame = GameData.CurrentGame;
        //        if (CurrentGame.GetGameEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameEXENameX32() + ".exe")))
        //        {
        //            executablesCount++;
        //            exeEqual++;
        //            customs.Add(executablesCount + @"\title", CurrentGame.GetGameEXENameX32());
        //            customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameEXENameX32() + ".exe"));
        //            customs.Add(executablesCount + @"\workingDirectory", ManageSettings.GetCurrentGameDataPath());
        //        }
        //        if (CurrentGame.GetGameStudioEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe")))
        //        {
        //            executablesCount++;
        //            customs.Add(executablesCount + @"\title", CurrentGame.GetGameStudioEXENameX32());
        //            customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe"));
        //            customs.Add(executablesCount + @"\workingDirectory", ManageSettings.GetCurrentGameDataPath());
        //        }
        //    }
        //    else
        //    {
        //        foreach (var value in ManagePython.GetExecutableInfosFromPyPlugin())
        //        {
        //            if (string.IsNullOrWhiteSpace(value.Key) || !File.Exists(value.Value))
        //            {
        //                continue;
        //            }

        //            executablesCount++;
        //            customs.Add(executablesCount + @"\title", value.Key);
        //            customs.Add(executablesCount + @"\binary", value.Value);
        //            customs.Add(executablesCount + @"\arguments", string.Empty);
        //            customs.Add(executablesCount + @"\workingDirectory", Path.GetDirectoryName(value.Value));
        //            customs.Add(executablesCount + @"\ownicon", "true");
        //        }
        //    }

        //    string[] pathExclusions =
        //        {
        //        "BepInEx" + Path.DirectorySeparatorChar + "plugins",
        //        //"Lec.ExtProtocol",
        //        //"ezTransXP.ExtProtocol",
        //        ".ExtProtocol",
        //        //"Common.ExtProtocol.Executor",
        //        "UnityCrashHandler64",
        //        Path.DirectorySeparatorChar + "IPA",
        //        "WideSliderPatch"
        //        };

        //    {
        //        //Добавление exe из Data
        //        //Parallel.ForEach(Directory.EnumerateFileSystemEntries(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories),
        //        //    exePath =>
        //        //    {
        //        //        try
        //        //        {
        //        //            if (exeEqual > 9)
        //        //            {
        //        //                return;
        //        //            }

        //        //            var exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //            {
        //        //                executablesCount++;
        //        //                if (!OneexeNotEqual)
        //        //                {
        //        //                    if (exists.Contains(exePath))
        //        //                    {
        //        //                        exeEqual++;
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        exeEqual = 0;
        //        //                        OneexeNotEqual = true;
        //        //                    }
        //        //                }

        //        //                customs.Add(executablesCount + @"\title", exeName);
        //        //                customs.Add(executablesCount + @"\binary", exePath);
        //        //            }
        //        //        }
        //        //        catch { }

        //        //    });
        //    }

        //    foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetCurrentGameDataPath(), "*.exe", SearchOption.AllDirectories))
        //    {
        //        try
        //        {
        //            if (exeEqual > 9)
        //            {
        //                return;
        //            }

        //            var exeName = Path.GetFileNameWithoutExtension(exePath);
        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && Path.GetDirectoryName(exePath) != ManageSettings.GetCurrentGameDataPath() && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //            {
        //                executablesCount++;
        //                if (!OneexeNotEqual)
        //                {
        //                    if (exists.Contains(exePath))
        //                    {
        //                        exeEqual++;
        //                    }
        //                    else
        //                    {
        //                        exeEqual = 0;
        //                        OneexeNotEqual = true;
        //                    }
        //                }

        //                customs.Add(executablesCount + @"\title", exeName);
        //                customs.Add(executablesCount + @"\binary", exePath);
        //            }
        //        }
        //        catch { }
        //    }

        //    {
        //        //foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories))
        //        //{
        //        //    string exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //    if (exeName.Length > 0 && !customExecutables.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //    {
        //        //        ExecutablesCount++;
        //        //        customExecutables.Add(ExecutablesCount + @"\title", exeName);
        //        //        customExecutables.Add(ExecutablesCount + @"\binary", exePath);
        //        //    }
        //        //}
        //        //Добавление exe из Mods
        //        //Parallel.ForEach(Directory.EnumerateFileSystemEntries(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories),
        //        //    exePath =>
        //        //    {
        //        //        try
        //        //        {
        //        //            if (exeEqual > 9)
        //        //            {
        //        //                return;
        //        //            }

        //        //            string exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //            {
        //        //                executablesCount++;
        //        //                if (!OneexeNotEqual)
        //        //                {
        //        //                    if (exists.Contains(exePath))
        //        //                    {
        //        //                        exeEqual++;
        //        //                    }
        //        //                    else
        //        //                    {
        //        //                        exeEqual = 0;
        //        //                        OneexeNotEqual = true;
        //        //                    }
        //        //                }

        //        //                customs.Add(executablesCount + @"\title", exeName);
        //        //                customs.Add(executablesCount + @"\binary", exePath);
        //        //            }
        //        //        }
        //        //        catch { }

        //        //    });
        //        //foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories))
        //        //{
        //        //    string exeName = Path.GetFileNameWithoutExtension(exePath);
        //        //    if (Path.GetFileNameWithoutExtension(exePath).Length > 0 && !customExecutables.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //        //    {
        //        //        ExecutablesCount++;
        //        //        customExecutables.Add(ExecutablesCount + @"\title", exeName);
        //        //        customExecutables.Add(ExecutablesCount + @"\binary", exePath);
        //        //    }
        //        //}
        //    }

        //    foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetCurrentGameModsPath(), "*.exe", SearchOption.AllDirectories))
        //    {
        //        try
        //        {
        //            if (exeEqual > 9)
        //            {
        //                return;
        //            }

        //            string exeName = Path.GetFileNameWithoutExtension(exePath);
        //            if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
        //            {
        //                executablesCount++;
        //                if (!OneexeNotEqual)
        //                {
        //                    if (exists.Contains(exePath))
        //                    {
        //                        exeEqual++;
        //                    }
        //                    else
        //                    {
        //                        exeEqual = 0;
        //                        OneexeNotEqual = true;
        //                    }
        //                }

        //                customs.Add(executablesCount + @"\title", exeName);
        //                customs.Add(executablesCount + @"\binary", exePath);
        //            }
        //        }
        //        catch { }
        //    }

        //    //добавление hardcoded exe
        //    //executablesCount++;
        //    //customs.Add(executablesCount + @"\title", "Skyrim");
        //    //customs.Add(executablesCount + @"\binary", Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "TESV.exe"));
        //    //customs.Add(executablesCount + @"\workingDirectory", Properties.Settings.Default.ApplicationStartupPath);
        //    //customs.Add(executablesCount + @"\ownicon", "true");
        //    //executablesCount++;
        //    //customs.Add(executablesCount + @"\title", "Explore Virtual Folder");
        //    //customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetMOdirPath(), "explorer++", "Explorer++.exe"));
        //    //customs.Add(executablesCount + @"\workingDirectory", Path.Combine(ManageSettings.GetMOdirPath(), "explorer++"));
        //    //customs.Add(executablesCount + @"\arguments", ManageSettings.GetCurrentGameDataPath());
        //    //customs.Add(executablesCount + @"\ownicon", "true");

        //    customExecutables = customs;
        //    ExecutablesCount = executablesCount;
        //}

        private static void SetCommonIniValues(INIFile ini)
        {
            {
                //string[,] iniValuesOld =
                //    {
                //        //General
                //        {
                //            "@ByteArray("+ManageSettings.GetCurrentGamePath()+")"
                //            ,
                //            "General"
                //            ,
                //            "gamePath"
                //        }
                //    //,
                //    //    {
                //    //        GetSelectedProfileName()
                //    //        ,
                //    //        "General"
                //    //        ,
                //    //        "@ByteArray("+"selected_profile"+")"
                //    //    }


                //    //,
                //    //    {
                //    //        "1"
                //    //        ,
                //    //        "General"
                //    //        ,
                //    //        @"selected_executable"
                //    //    }


                //    //,
                //    //    //customExecutables
                //    //    {
                //    //        SettingsManage.GetCurrentGameEXEName()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"1\title"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetDataPath(), SettingsManage.GetCurrentGameEXEName() + ".exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"1\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"1\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetINISettingsEXEName()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"2\title"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetSettingsEXEPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"2\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"2\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++", "Explorer++.exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"3\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"3\arguments"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetMOdirPath(), "explorer++")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"3\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetCurrentGamePath(), "TESV.exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"4\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetCurrentGamePath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"4\workingDirectory"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetStudioEXEName())
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"5\title"
                //    //    }
                //    //,
                //    //    {
                //    //        Path.Combine(SettingsManage.GetDataPath(),SettingsManage.GetStudioEXEName()+".exe")
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"5\binary"
                //    //    }
                //    //,
                //    //    {
                //    //        SettingsManage.GetDataPath()
                //    //        ,
                //    //        "customExecutables"
                //    //        ,
                //    //        @"5\workingDirectory"
                //    //    }
                //    ,
                //        //Settings
                //        {
                //            ManageSettings.GetCurrentGameModsPath()
                //            ,
                //            "Settings"
                //            ,
                //            @"mod_directory"
                //        }
                //    ,
                //        {
                //            Path.Combine(ManageSettings.GetCurrentGamePath(), "Downloads")
                //            ,
                //            "Settings"
                //            ,
                //            @"download_directory"
                //        }
                //    ,
                //        {
                //            Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles")
                //            ,
                //            "Settings"
                //            ,
                //            @"profiles_directory"
                //        }
                //    ,
                //        {
                //            Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "overwrite")
                //            ,
                //            "Settings"
                //            ,
                //            @"overwrite_directory"
                //        }
                //    ,
                //        {
                //            CultureInfo.CurrentCulture.Name.Split('-')[0]
                //            ,
                //            "Settings"
                //            ,
                //            @"language"
                //        }

                //    };
            }

            string[,] iniValues =
                {
                    //General
                    {
                        "@ByteArray("+ManageSettings.GetCurrentGamePath()+")"
                        ,
                        "General"
                        ,
                        "gamePath"
                    }
                ,
                    //Settings
                    {
                        ManageSettings.GetCurrentGameModsDirPath()
                        ,
                        "Settings"
                        ,
                        "mod_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGamePath(), "Downloads")
                        ,
                        "Settings"
                        ,
                        "download_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles")
                        ,
                        "Settings"
                        ,
                        "profiles_directory"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "overwrite")
                        ,
                        "Settings"
                        ,
                        "overwrite_directory"
                    }
                ,
                    {
                        CultureInfo.CurrentCulture.Name.Split('-')[0]
                        ,
                        "Settings"
                        ,
                        "language"
                    }
                ,
                    {
                        "false"
                        ,
                        "Settings"
                        ,
                        "check_for_updates" // disable check for updates by mo to prevent autoupdate and possible data lost. Update button will make buckups before update and will update more safe.
                    }

                };

            int iniValuesLength = iniValues.Length / 3;

            bool changed = false;
            for (int i = 0; i < iniValuesLength; i++)
            {
                string subquote = iniValues[i, 2].EndsWith(@"\arguments", StringComparison.InvariantCulture) ? "\\\"" : string.Empty;
                string keyValue = iniValues[i, 0].Replace(@"\", @"\\") + subquote;
                string sectionName = iniValues[i, 1];
                string keyName = iniValues[i, 2];

                if (ini.GetKey(sectionName, keyName) != keyValue)
                {
                    changed = true;
                    ini.SetKey(sectionName, keyName, keyValue, false);
                }
            }

            if (changed)
            {
                ini.WriteFile();
            }
        }

        //internal static Dictionary<string, string> GetMOcustomExecutablesList()
        //{
        //    var INI = ManageIni.GetINIFile(ManageSettings.GetMOiniPath());
        //    var customs = INI.ReadSectionKeyValuePairsToDictionary("customExecutables");
        //    var retDict = new Dictionary<string, string>();
        //    foreach (var pair in customs)
        //    {
        //        if (pair.Key.EndsWith(@"\title", StringComparison.InvariantCulture))
        //        {
        //            retDict.Add(pair.Value, customs[pair.Key.Split('\\')[0] + @"\binary"]);
        //        }
        //    }
        //    return retDict;
        //}

        /// <summary>
        /// get title of custom executable by it exe name
        /// </summary>
        /// <param name="exename"></param>
        /// <param name="ini"></param>
        /// <returns></returns>
        internal static string GetMOcustomExecutableTitleByExeName(string exename, INIFile ini = null, bool newMethod = true)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.GetMOiniPath());
            }

            if (newMethod)
            {
                var customs = new CustomExecutables(ini);
                foreach (var customExe in customs.List)
                {
                    if (Path.GetFileNameWithoutExtension(customExe.Value.Binary) == exename)
                    {
                        if (File.Exists(customExe.Value.Binary))
                        {
                            return customExe.Value.Title;
                        }
                    }
                }
            }
            else
            {
                var customs = ini.GetSectionValuesToDictionary("customExecutables");
                foreach (var pair in customs)
                {
                    if (pair.Key.EndsWith(@"\binary", StringComparison.InvariantCulture))
                        if (Path.GetFileNameWithoutExtension(pair.Value) == exename)
                            if (File.Exists(pair.Value))
                            {
                                return customs[pair.Key.Split('\\')[0] + @"\title"];
                            }
                }
            }

            return exename;
        }

        /// <summary>
        /// inserts in MO.ini new custom executable
        /// required exeParams 0 is exe title, required exeParams 1 is exe bynary path
        /// optional exeParams 2 is arguments, optional exeParams 4 is working directory
        /// </summary>
        /// <param name="newCustomExecutable"></param>
        internal static void InsertCustomExecutable(CustomExecutables.CustomExecutable newCustomExecutable, INIFile ini = null, bool insertOnlyMissingBinary = true)
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.GetMOiniPath());
            }

            var customExcutables = new CustomExecutables(ini);
            string customToUpdate = "";
            if (insertOnlyMissingBinary)
            {
                foreach (var exe in customExcutables.List)
                {
                    if (exe.Value.Binary == newCustomExecutable.Binary && exe.Value.Title == newCustomExecutable.Title) // return if exe found and have same title
                    {
                        customToUpdate = exe.Key; // memorize found custom exe number
                        break;
                    }
                }
            }

            if (customToUpdate.Length == 0)
            {
                customExcutables.List.Add(customExcutables.List.Count + 1 + "", newCustomExecutable);
            }
            else
            {
                // update same record from input instead of add new
                foreach (var attributeName in customExcutables.List[customToUpdate].Attribute.Keys.ToList())
                {
                    if (customExcutables.List[customToUpdate].Attribute[attributeName] != newCustomExecutable.Attribute[attributeName])
                    {
                        customExcutables.List[customToUpdate].Attribute[attributeName] = newCustomExecutable.Attribute[attributeName];
                    }
                }
            }

            customExcutables.Save();
        }

        /// <summary>
        /// get MO custom executables count
        /// </summary>
        /// <param name="customs"></param>
        /// <returns></returns>
        internal static int GetMOiniCustomExecutablesCount(Dictionary<string, string> customs = null)
        {
            customs = customs ?? ManageIni.GetINIFile(ManageSettings.GetMOiniPath()).GetSectionValuesToDictionary("customExecutables");

            if (customs.Count == 0)//check if caustoms is exists
            {
                return 0;
            }

            int ind = 1;
            if (!customs.ContainsKey(ind + @"\binary"))//return 0 if there is no binary in list
            {
                return 0;
            }

            while (customs.ContainsKey(ind + @"\binary"))//iterate while binary with index is exist
            {
                ind++;
            }
            return ind - 1;
        }

        /// <summary>
        /// returns true if program custom exe is exists in MO list
        /// </summary>
        /// <param name="exename"></param>
        /// <returns></returns>
        internal static bool IsMOcustomExecutableTitleByExeNameExists(string exename)
        {
            var customs = ManageIni.GetINIFile(ManageSettings.GetMOiniPath()).GetSectionValuesToDictionary("customExecutables");
            foreach (var pair in customs)
            {
                if (pair.Key.EndsWith(@"\binary", StringComparison.InvariantCulture))
                    if (Path.GetFileNameWithoutExtension(pair.Value) == exename)
                        if (File.Exists(pair.Value))
                        {
                            return true;
                        }
            }
            return false;
        }

        public static void ActivateMod(string modname)
        {
            ActivateDeactivateInsertMod(modname);
        }

        /// <summary>
        /// check symlink for usedata dir when target is not exists or symlink is not valid. overwrite for mo mode and data for common mode
        /// </summary>
        internal static void CheckMoUserdata()
        {
            try
            {
                DirectoryInfo objectDir;
                //overwrite dir of mo folder for mo mode and data folder for common mode
                if (ManageSettings.IsMoMode())
                {
                    objectDir = new DirectoryInfo(ManageSettings.GetCurrentGameMoOverwritePath());
                }
                else
                {
                    objectDir = new DirectoryInfo(ManageSettings.GetCurrentGameDataPath());
                }

                //create target object dir when it is not exists
                if (!objectDir.Exists)
                {
                    objectDir.Create();
                }

                //symlink path for MOUserData in game's dir
                DirectoryInfo symlinkPath = new DirectoryInfo(ManageSettings.GetOverwriteFolderLink());

                //delete if target is not exists or not symlink and recreate
                if (!symlinkPath.IsValidSymlinkTargetEquals(objectDir.FullName))
                {
                    if (symlinkPath.Exists)
                    {
                        symlinkPath.Delete();
                    }

                    objectDir.CreateSymlink(symlinkPath.FullName, true);
                }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occured in CheckMoUserdata. error:" + ex);
            }
        }

        public static void DeactivateMod(string modname)
        {
            ActivateDeactivateInsertMod(modname, false);
        }

        /// <summary>
        /// Insert <paramref name="modname"/> in the current mod organizer's profile's modlist.txt.
        /// If <paramref name="modAfterWhichInsert"/> is not set new <paramref name="modname"/> will be placed at the end of modlist.
        /// </summary>
        /// <param name="modname"></param>
        /// <param name="activate">If need the <paramref name="modname"/> was activated in modlist</param>
        /// <param name="modAfterWhichInsert">Name of mod after which must be inserted else will me inserted in the end od list.</param>
        /// <param name="placeAfter"> Will determine before or after of <paramref name="modAfterWhichInsert"/> new <paramref name="modname"/> must be inserted</param>
        public static void InsertMod(string modname, bool activate = true, string modAfterWhichInsert = "", bool placeAfter = true)
        {
            ActivateDeactivateInsertMod(modname, activate, modAfterWhichInsert, placeAfter);
        }

        /// <summary>
        /// Insert <paramref name="modname"/> in the current mod organizer's profile's modlist.txt.
        /// </summary>
        /// <param name="modname"></param>
        /// <param name="activate">If need the <paramref name="modname"/> was activated in modlist</param>
        /// <param name="recordWithWhichInsert">Name of mod after which must be inserted else will me inserted in the end od list.</param>
        /// <param name="placeAfter"> Will determine before or after of <paramref name="recordWithWhichInsert"/> new <paramref name="modname"/> must be inserted</param>
        public static void ActivateDeactivateInsertMod(string modname, bool activate = true, string recordWithWhichInsert = "", bool placeAfter = true)
        {
            // insert new
            var modList = new ProfileModlist();

            // just activate/deactivate if exists
            if (modList.ItemByName.ContainsKey(modname))
            {
                modList.ItemByName[modname].IsEnabled = activate;
                modList.Save();
                return;
            }

            var record = new ProfileModlistRecord
            {
                IsEnabled = activate,
                IsSeparator = false,
                Name = modname,
                Path = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modname)
            };
            record.IsExist = Directory.Exists(record.Path);


            if (!string.IsNullOrWhiteSpace(recordWithWhichInsert)
                && recordWithWhichInsert.EndsWith("_separator", comparisonType: StringComparison.InvariantCulture))
            {
                record.ParentSeparator = new ProfileModlistRecord
                {
                    IsSeparator = true,
                    Name = recordWithWhichInsert,
                    Path = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), recordWithWhichInsert)
                };
                record.ParentSeparator.IsExist = Directory.Exists(record.ParentSeparator.Path);
            }

            modList.Insert(record, (record.ParentSeparator == null && !string.IsNullOrWhiteSpace(recordWithWhichInsert) ? recordWithWhichInsert : ""), placeAfter);
            modList.Save();

            //if (modname.Length > 0)
            //{
            //    string currentMOprofile = ManageSettings.GetMoSelectedProfileDirName();

            //    if (currentMOprofile.Length == 0)
            //    {
            //    }
            //    else
            //    {
            //        string profilemodlistpath = ManageSettings.GetCurrentMoProfileModlistPath();

            //        InsertLineInModlistFile(profilemodlistpath, (activate ? "+" : "-") + modname, 1, recordWithWhichInsert, placeAfter);
            //    }
            //}
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="moModlistPath"></param>
        /// <param name="line"></param>
        /// <param name="position"></param>
        public static void InsertLineInModlistFile(string moModlistPath, string line, int position = 1, string insertWithThisMod = "", bool placeAfter = true)
        {
            if (moModlistPath.Length > 0 && File.Exists(moModlistPath) && line.Length > 0)
            {
                string[] fileLines = File.ReadAllLines(moModlistPath);
                bool isEnabled;
                if (!(isEnabled = fileLines.Contains("+" + line.Remove(0, 1))) && !fileLines.Contains("-" + line.Remove(0, 1)))
                {
                    int fileLinesLength = fileLines.Length;
                    bool insertWithMod = insertWithThisMod.Length > 0;
                    position = insertWithMod ? fileLinesLength : position;
                    using (StreamWriter writer = new StreamWriter(moModlistPath))
                    {
                        for (int lineNumber = 0; lineNumber < position; lineNumber++)
                        {
                            if (insertWithMod && fileLines[lineNumber].Length > 0 && string.Compare(fileLines[lineNumber].Remove(0, 1), insertWithThisMod, true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (placeAfter)
                                {
                                    position = lineNumber;
                                }
                                else
                                {
                                    writer.WriteLine(fileLines[lineNumber]);
                                    position = lineNumber + 1;
                                }
                                break;

                            }
                            else
                            {
                                writer.WriteLine(fileLines[lineNumber]);
                            }
                        }

                        writer.WriteLine(line);

                        if (position < fileLinesLength)
                        {
                            for (int lineNumber = position; lineNumber < fileLinesLength; lineNumber++)
                            {
                                writer.WriteLine(fileLines[lineNumber]);
                            }
                        }
                    }
                }
                else
                {
                    var prefix = "-";
                    if (isEnabled)
                    {
                        prefix = "+";
                    }
                    fileLines = fileLines.Replace(prefix + line.Remove(0, 1), line);
                    File.WriteAllLines(moModlistPath, fileLines);
                }
            }
        }

        /// <summary>
        /// Writes required parameters in meta.ini
        /// </summary>
        /// <param name="moddir"></param>
        /// <param name="categoryNames">names of categories splitted by ','</param>
        /// <param name="version"></param>
        /// <param name="comments"></param>
        /// <param name="notes"></param>
        public static void WriteMetaIni(string moddir, string categoryNames = "", string version = "", string comments = "", string notes = "")
        {
            new MetaIniInfo().Set(moddir, categoryNames, version, comments, notes);
        }

        /// <summary>
        /// restore modlist which was not restored after zipmods update
        /// </summary>
        internal static void RestoreModlist()
        {
            if (File.Exists(ManageSettings.GetCurrentMoProfileModlistPath() + ".prezipmodsUpdate"))
            {
                try
                {
                    File.Move(ManageSettings.GetCurrentMoProfileModlistPath(), ManageSettings.GetCurrentMoProfileModlistPath() + ".tmp");

                    if (!File.Exists(ManageSettings.GetCurrentMoProfileModlistPath()))
                        File.Move(ManageSettings.GetCurrentMoProfileModlistPath() + ".prezipmodsUpdate", ManageSettings.GetCurrentMoProfileModlistPath());

                    if (File.Exists(ManageSettings.GetCurrentMoProfileModlistPath()))
                    {
                        new FileInfo(ManageSettings.GetCurrentMoProfileModlistPath() + ".tmp").DeleteReadOnly();
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("RestoreModlist error:\r\n" + ex);
                }
            }
        }

        /// <summary>
        /// return list of mo names in active profile folder
        /// </summary>
        /// <param name="onlyEnabled"></param>
        /// <returns></returns>
        public static string[] GetModNamesListFromActiveMoProfile(bool onlyEnabled = true)
        {
            string currentMOprofile = ManageSettings.GetMoSelectedProfileDirName();

            if (currentMOprofile.Length > 0)
            {
                string profilemodlistpath = ManageSettings.GetCurrentMoProfileModlistPath();

                //фикс на случай несовпадения выбранной игры и профиля в MO ini
                if (!File.Exists(profilemodlistpath))
                {
                    RedefineGameMoData();
                    currentMOprofile = ReGetcurrentMOprofile(currentMOprofile);

                    profilemodlistpath = ManageSettings.GetCurrentMoProfileModlistPath();
                }

                if (File.Exists(profilemodlistpath))
                {
                    string[] lines;
                    if (onlyEnabled)
                    {
                        //все строки с + в начале
                        lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && line[0] == '+').ToArray();
                    }
                    else
                    {
                        //все строки кроме сепараторов
                        lines = File.ReadAllLines(profilemodlistpath).Where(line => line.Length > 0 && line[0] != '#' && (line.Length < 10 || !line.EndsWith("_separator", StringComparison.InvariantCulture))).ToArray();
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

            return new string[1];
        }

        private static string ReGetcurrentMOprofile(string currentMOprofile)
        {
            while (!Directory.Exists(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile)))
            {
                if (Directory.Exists(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", ManageSettings.GetCurrentGameExeName())))
                {
                    return ManageSettings.GetCurrentGameExeName();
                }
                else if (Directory.Exists(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", ManageSettings.GetCurrentGameDisplayingName())))
                {
                    return ManageSettings.GetCurrentGameDisplayingName();
                }
                else if (Directory.Exists(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", ManageSettings.GetCurrentGameFolderName())))
                {
                    return ManageSettings.GetCurrentGameFolderName();
                }
                else if (Directory.GetDirectories(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles")).Length == 0)
                {
                    MessageBox.Show(T._("No profiles found for Current game") + " " + ManageSettings.GetCurrentGameDisplayingName());
                }
                else
                {
                    return Path.GetDirectoryName(Directory.GetDirectories(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles"))[0]);
                }
            }
            Application.Exit();
            return "Default";
        }

        public static string GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(string[] pathInMods, bool[] isDir)
        {
            string path = string.Empty;
            int d = 0;
            try
            {
                if (pathInMods != null)
                    foreach (var pathCandidate in pathInMods)
                    {
                        path = pathCandidate;

                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            if ((isDir[d] && Directory.Exists(pathCandidate)) || (!isDir[d] && File.Exists(pathCandidate)))
                            {
                                return pathCandidate;
                            }
                            else
                            {
                                path = GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(pathCandidate, isDir[d]);
                                if (!string.IsNullOrWhiteSpace(path) && path != pathCandidate)
                                {
                                    return path;
                                }
                            }
                        }

                        d++;
                    }
            }
            catch (Exception ex)
            {
                ManageLogs.Log("An error occured while path get:\r\n" + ex + "\r\npath=" + path);
            }

            return path;
        }

        public static string GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(string pathInMods, bool isDir = false, bool onlyEnabled = true)
        {
            if (string.IsNullOrWhiteSpace(pathInMods))
            {
                return pathInMods;
            }

            try
            {
                string modsOverwrite = pathInMods.Contains(ManageSettings.GetCurrentGameMoOverwritePath()) ? ManageSettings.GetCurrentGameMoOverwritePath() : ManageSettings.GetCurrentGameModsDirPath();

                //искать путь только для ссылки в Mods или в Data
                if (!ManageStrings.IsStringAContainsStringB(pathInMods, modsOverwrite) && !ManageStrings.IsStringAContainsStringB(pathInMods, ManageSettings.GetCurrentGameDataPath()))
                    return pathInMods;

                //отсеивание первого элемента с именем мода
                //string subpath = string.Empty;

                string[] pathInModsElements = pathInMods
                    .Replace(modsOverwrite, string.Empty)
                    .Split(Path.DirectorySeparatorChar)
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                //для мода в mods пропустить имя этого мода
                if (modsOverwrite == ManageSettings.GetCurrentGameModsDirPath())
                {
                    pathInModsElements = pathInModsElements.Skip(1).ToArray();
                }

                //foreach (var element in pathInModsElements)
                //{
                //    if (i > 1)
                //    {
                //        subpath += i > 2 ? Path.DirectorySeparatorChar.ToString() : string.Empty;
                //        subpath += element;
                //    }
                //    i++;
                //}
                string subpath = string.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), pathInModsElements);

                if (!ManageSettings.IsMoMode())
                {
                    return Path.Combine(ManageSettings.GetCurrentGameDataPath(), subpath);
                }

                //check in Overwrite 1st
                string overwritePath = ManageSettings.GetCurrentGameMoOverwritePath() + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + subpath;
                if (isDir)
                {
                    if (Directory.Exists(overwritePath))
                    {
                        return overwritePath;
                    }
                }
                else
                {
                    if (File.Exists(overwritePath))
                    {
                        return overwritePath;
                    }
                }

                //поиск по списку активных модов
                string modsPath = ManageSettings.GetCurrentGameModsDirPath();
                var modNames = GetModNamesListFromActiveMoProfile(onlyEnabled);
                foreach (var modName in modNames)
                {
                    string possiblePath = Path.Combine(modsPath, modName) + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + subpath;
                    if (isDir)
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
            }
            catch
            {
            }

            return pathInMods;
        }

        /// <summary>
        /// Gets setup.xml path from latest enabled mod like must be in Mod Organizer
        /// </summary>
        /// <returns></returns>
        public static string GetSetupXmlPathForCurrentProfile()
        {
            if (ManageSettings.IsMoMode())
            {

                return GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(Path.Combine(ManageSettings.GetOverwriteFolder(), "UserData", "setup.xml"));
            }
            else
            {
                return Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "setup.xml");
            }
        }

        public static string GetMetaParameterValue(string metaFilePath, string neededValue)
        {
            return ManageIni.GetIniValueIfExist(metaFilePath, neededValue);

            //using (StreamReader sr = new StreamReader(MetaFilePath))
            //{
            //    string line;
            //    while (!sr.EndOfStream)
            //    {
            //        line = sr.ReadLine();

            //        if (line.Length > 0)
            //        {
            //            if (line.StartsWith(NeededValue + "=", StringComparison.InvariantCulture))
            //            {
            //                return line.Remove(0, (NeededValue + "=").Length);
            //            }
            //        }
            //    }
            //}

            //return string.Empty;
        }

        public static string GetModFromModListContainsTheName(string name, bool onlyFromEnabledMods = true)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string[] modList = GetModNamesListFromActiveMoProfile(onlyFromEnabledMods);
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

        /// <summary>
        /// для МО до версии 2.3 -создание болванки, для 2.3 и новее - удаление, если есть
        /// </summary>
        public static void DummyFiles()
        {
            if (ManageSettings.MoIsNew)
            {
                // remove dummy file
                if (File.Exists(ManageSettings.GetDummyFilePath()))
                {
                    File.Delete(ManageSettings.GetDummyFilePath());
                }

                var ini = ManageIni.GetINIFile(ManageSettings.GetModOrganizerIniPath());

                RemoveCustomExecutable("Skyrim", ini);

                // change gameName to specific mo plugin set
                if (ini.GetKey("General", "gameName") == "Skyrim")
                {
                    ini.SetKey("General", "gameName", GetMoBasicGamePluginGameName());
                }
            }
            else
            {
                //Create dummy file and add hidden attribute
                if (!File.Exists(ManageSettings.GetDummyFilePath()))
                {
                    File.Copy(ManageSettings.GetDummyFileResPath(), ManageSettings.GetDummyFilePath());
                    //new FileInfo(ManageSettings.GetDummyFilePath()).Create().Close();
                    //File.WriteAllText(ManageSettings.GetDummyFilePath(), "dummy file need to execute mod organizer");
                    ManageFilesFolders.HideFileFolder(ManageSettings.GetDummyFilePath(), true);
                }
            }
        }

        /// <summary>
        /// remove custom executable by selected attribute
        /// </summary>
        /// <param name="ini"></param>
        /// <param name="AttributeToCheck"></param>
        private static void RemoveCustomExecutable(string keyToFind, INIFile ini = null, string AttributeToCheck = "title")
        {
            if (ini == null)
            {
                ini = ManageIni.GetINIFile(ManageSettings.GetModOrganizerIniPath());
            }

            var customs = new CustomExecutables(ini);
            foreach (var custom in customs.List)
            {
                if (custom.Value.Attribute[AttributeToCheck] == keyToFind)
                {
                    customs.listToRemoveKeys.Add(custom.Key);
                }
            }

            customs.Save();
        }

        public static string GetCategoryNameForTheIndex(string inputCategoryIndex, CategoriesInfo categoriesList = null)
        {
            return GetCategoryIndexNameBase(inputCategoryIndex, categoriesList, true);
        }

        public static string GetCategoryIndexForTheName(string inputCategoryNames, CategoriesInfo categoriesList = null)
        {
            return GetCategoryIndexNameBase(inputCategoryNames, categoriesList, false);
        }

        /// <summary>
        /// GetName = true means will be returned categorie name by index else will be returned index by name
        /// </summary>
        /// <param name="inputNamesOrIds"></param>
        /// <param name="inputCategoriesList"></param>
        /// <param name="getName"></param>
        /// <returns></returns>
        public static string GetCategoryIndexNameBase(string inputNamesOrIds, CategoriesInfo inputCategoriesList = null, bool getName = false)
        {
            var categoriesList = inputCategoriesList ?? new CategoriesInfo();

            List<string> categoryParts = new List<string>();
            foreach (var part in inputNamesOrIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var category in categoriesList.Records)
                {
                    if ((getName ? category.ID : category.Name) == part)
                    {
                        categoryParts.Add(getName ? category.Name : category.ID);
                    }
                }
            }

            return categoryParts.Count == 0 ? "" : categoryParts.Count > 1 ? string.Join(",", categoryParts) : categoryParts[0].Length > 0 ? categoryParts[0] + "," : "";
        }

        public static string GetCategorieNamesForTheFolder(string modDir, string defaultCategory, CategoriesInfo categoriesList = null)
        {
            string resultCategory = defaultCategory;

            if (categoriesList == null)
            {
                categoriesList = new CategoriesInfo();
            }

            string[,] categorieRules =
            {
                { Path.Combine(modDir, "BepInEx", "Plugins"), GetCategoryIndexForTheName("Plugins",categoriesList), "dll" } //Plug-ins 51
                ,
                { Path.Combine(modDir, "UserData"), GetCategoryIndexForTheName("UserFiles",categoriesList), "*" } //UserFiles 53
                ,
                { Path.Combine(modDir, "UserData", "chara"), GetCategoryIndexForTheName("Characters",categoriesList), "png" } //Characters 54
                ,
                { Path.Combine(modDir, "UserData", "studio", "scene"), GetCategoryIndexForTheName("Studio scenes",categoriesList), "png"} //Studio scenes 57
                ,
                { Path.Combine(modDir, "Mods"), GetCategoryIndexForTheName("Sideloader",categoriesList), "zip" } //Sideloader 60
                ,
                { Path.Combine(modDir, "scripts"), GetCategoryIndexForTheName("ScriptLoader scripts",categoriesList), "cs"} //ScriptLoader scripts 86
                ,
                { Path.Combine(modDir, "UserData", "coordinate"), GetCategoryIndexForTheName("Coordinate",categoriesList), "png"} //Coordinate 87
                ,
                { Path.Combine(modDir, "UserData", "Overlays"), GetCategoryIndexForTheName("Overlay",categoriesList), "png"} //Overlay 88
                ,
                { Path.Combine(modDir, "UserData", "housing"), GetCategoryIndexForTheName("Housing",categoriesList), "png"} //Housing 89
                ,
                { Path.Combine(modDir, "UserData", "housing"), GetCategoryIndexForTheName("Cardframe",categoriesList), "png"} //Cardframe 90
            };

            int categorieRulesLength = categorieRules.Length / 3;
            for (int i = 0; i < categorieRulesLength; i++)
            {
                string dir = categorieRules[i, 0];
                string categorieNum = categorieRules[i, 1];
                string extension = categorieRules[i, 2];
                if (
                    (
                        (defaultCategory.Length > 0
                        && !defaultCategory.Contains("," + categorieNum)
                        && !defaultCategory.Contains(categorieNum + ",")
                        )
                     || defaultCategory.Length == 0
                    )
                    && Directory.Exists(dir)
                    && !dir.IsNullOrEmptyDirectory()
                    && ManageFilesFolders.IsAnyFileExistsInTheDir(dir, extension)
                   )
                {
                    if (resultCategory.Length > 0)
                    {
                        if (resultCategory.Substring(resultCategory.Length - 1, 1) == ",")
                        {
                            resultCategory += categorieNum;
                        }
                        else
                        {
                            resultCategory += "," + categorieNum;
                        }
                    }
                    else
                    {
                        resultCategory = categorieNum + ",";
                    }
                }
            }

            return resultCategory;
        }

        internal static void MoIniFixes()
        {
            if (!File.Exists(ManageSettings.GetModOrganizerIniPath())) return;

            var ini = ManageIni.GetINIFile(ManageSettings.GetModOrganizerIniPath());
            //if (INI == null) return;

            string gameName;
            //updated game name
            if (string.IsNullOrWhiteSpace(gameName = ini.GetKey("General", "gameName")) || gameName != ManageSettings.GetMoCurrentGameName())
            {
                ini.SetKey("General", "gameName", ManageSettings.GetMoCurrentGameName(), false);
            }

            //clear pluginBlacklist section of MO ini to prevent plugin_python.dll exist there
            if (ini.SectionExistsAndNotEmpty("pluginBlacklist"))
            {
                ini.DeleteSection("pluginBlacklist", false);
            }

            bool selectedExecutableNeedToSet = true;
            var customs = new CustomExecutables(ini);
            foreach (var custom in customs.List)
            {
                // fix spaces in exe title to prevent errors because it
                if (custom.Value.Title.IndexOf(' ') != -1)
                {
                    custom.Value.Title = custom.Value.Title.Replace(' ', '_');
                }

                // Set selected_executable number to game exe
                if (selectedExecutableNeedToSet && Path.GetFileNameWithoutExtension(custom.Value.Binary) == CustomExecutables.NormalizePath(ManageSettings.GetCurrentGameExeName()))
                {
                    selectedExecutableNeedToSet = false;
                    var index = custom.Key;
                    ini.SetKey("General", "selected_executable", index, false);
                    ini.SetKey("Widgets", "MainWindow_executablesListBox_index", index, false);
                }

                // set target mod for kkmanager exe's
                if (!string.Equals(custom.Value.MoTargetMod, ManageSettings.KKManagerFilesModName(), StringComparison.InvariantCultureIgnoreCase) &&
                    (
                    Path.GetFileName(custom.Value.Binary) == ManageSettings.GetKkManagerExeName()
                    ||
                    Path.GetFileName(custom.Value.Binary) == ManageSettings.KkManagerStandaloneUpdaterExeName()
                    )
                    )
                {
                    custom.Value.MoTargetMod = ManageSettings.KKManagerFilesModName();
                }
            }

            customs.Save();

            ini.SetKey("PluginPersistance", @"Python%20Proxy\tryInit", "false");
        }

        /// <summary>
        /// clean MO folder from some useless files for illusion games
        /// </summary>
        internal static void CleanMoFolder()
        {
            var moFilesForClean = new[]
            {
                    @"MOFolder\plugins\bsa_*.dll",
                    //@"MOFolder\plugins\bsa_extractor.dll",
                    //@"MOFolder\plugins\bsa_packer.dll",
                    @"MOFolder\plugins\check_fnis.dll",
                    @"MOFolder\plugins\DDSPreview.py",
                    @"MOFolder\plugins\FNIS*.py",
                    //@"MOFolder\plugins\FNISPatches.py",
                    //@"MOFolder\plugins\FNISTool.py",
                    //@"MOFolder\plugins\FNISToolReset.py",
                    @"MOFolder\plugins\Form43Checker.py",
                    @"MOFolder\plugins\game_*.dll",
                    //@"MOFolder\plugins\game_enderal.dll",
                    //@"MOFolder\plugins\game_fallout3.dll",
                    //@"MOFolder\plugins\game_fallout4.dll",
                    //@"MOFolder\plugins\game_fallout4vr.dll",
                    //@"MOFolder\plugins\game_falloutNV.dll",
                    //@"MOFolder\plugins\game_morrowind.dll",
                    //@"MOFolder\plugins\game_oblivion.dll",
                    //@"MOFolder\plugins\game_skyrimse.dll",
                    //@"MOFolder\plugins\game_skyrimvr.dll",
                    //@"MOFolder\plugins\game_ttw.dll",
                    //@"MOFolder\plugins\game_skyrim.dll",
                    //@"MOFolder\plugins\game_enderalse.dll",
                    @"MOFolder\plugins\installer_bain.dll",
                    @"MOFolder\plugins\installer_bundle.dll",
                    @"MOFolder\plugins\installer_fomod.dll",
                    @"MOFolder\plugins\installer_ncc.dll",
                    @"MOFolder\plugins\installer_omod.dll",
                    @"MOFolder\plugins\preview_bsa.dll",
                    @"MOFolder\plugins\ScriptExtenderPluginChecker.py",
                    @"MOFolder\plugins\installer_fomod_csharp.dll",
                    @"MOFolder\plugins\data\OMODFramework*.*",
                    @"MOFolder\plugins\data\DDS\",
                    !GetMoVersion().StartsWith("2.3",StringComparison.InvariantCulture)?@"MOFolder\plugins\modorganizer-basic_games\":""
            };
            var mOfolderPath = ManageSettings.GetMOdirPath();
            foreach (var file in moFilesForClean)
            {
                var path = file.Replace("MOFolder", mOfolderPath);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (path.EndsWith("\\", StringComparison.InvariantCulture) && Directory.Exists(path))
                    {
                        var trimmedPath = path.TrimEnd('\\');
                        var searchDir = new DirectoryInfo(Path.GetDirectoryName(trimmedPath));
                        var searchPattern = Path.GetFileName(trimmedPath);
                        foreach (var foundDir in searchDir.EnumerateDirectories(searchPattern))
                        {
                            try
                            {
                                foundDir.Attributes = FileAttributes.Normal;
                                foundDir.Delete(true);
                            }
                            catch (Exception ex)
                            {
                                ManageLogs.Log("An error occured while MO dir cleaning from useless files. Error:\r\n" + ex);
                            }
                        }
                    }
                    else
                    {
                        var searchDir = new DirectoryInfo(Path.GetDirectoryName(path));
                        var searchPattern = Path.GetFileName(path);
                        foreach (var foundFile in searchDir.EnumerateFiles(searchPattern))
                        {
                            try
                            {
                                foundFile.Attributes = FileAttributes.Normal;
                                foundFile.Delete();
                            }
                            catch (Exception ex)
                            {
                                ManageLogs.Log("An error occured while MO dir cleaning from useless files. Error:\r\n" + ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// create game detection py files if missing
        /// </summary>
        internal static void CheckBaseGamesPy()
        {
            var moBaseGamesPluginGamesDirPath = ManageSettings.GetMoBaseGamesPluginGamesDirPath();
            if (!Directory.Exists(moBaseGamesPluginGamesDirPath))
            {
                return;
            }

            var py = GameData.CurrentGame.GetBaseGamePyFile();
            var pypath = Path.Combine(moBaseGamesPluginGamesDirPath, py.Name + ".py");
            if (!File.Exists(pypath) || py.Value.Length != new FileInfo(pypath).Length)
            {
                File.WriteAllBytes(pypath, py.Value);
            }
        }

        /// <summary>
        /// get GameName value frome basicgame plugin
        /// </summary>
        /// <returns></returns>
        internal static string GetMoBasicGamePluginGameName()
        {
            Match gameName = Regex.Match(Encoding.UTF8.GetString(GameData.CurrentGame.GetBaseGamePyFile().Value), @"GameName \= \""([^\""]+)\""");

            if (gameName == null)
            {
                return "";
            }

            return gameName.Result("$1");
        }
    }

    internal static class CustomExecutablesExtensions
    {
        /// <summary>
        /// Add new <paramref name="customExecutable"/> in the <paramref name="customExecutables"/>.
        /// </summary>
        /// <param name="customExecutables">Custom executables</param>
        /// <param name="customExecutable">Executable to add.</param>
        /// <param name="performSave">True if need to save right after add.</param>
        internal static void Add(this CustomExecutables customExecutables, CustomExecutables.CustomExecutable customExecutable, bool performSave = false)
        {
            customExecutables.List.Add((customExecutables.List.Count + 1) + "", customExecutable);
            if (performSave)
            {
                customExecutables.Save();
            }
        }

        /// <summary>
        /// Check if <paramref name="customExecutables"/> contains <paramref name="binaryPath"/>
        /// </summary>
        /// <param name="customExecutables"></param>
        /// <param name="binaryPath"></param>
        /// <returns></returns>
        internal static bool ContainsBinary(this CustomExecutables customExecutables, string binaryPath)
        {
            if (customExecutables == null || customExecutables.List == null || string.IsNullOrWhiteSpace(binaryPath))
            {
                return false;
            }

            foreach (var custom in customExecutables.List)
            {
                if (custom.Value.Binary == CustomExecutables.NormalizePath(binaryPath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if <paramref name="customExecutables"/> contains <paramref name="titleName"/>
        /// </summary>
        /// <param name="customExecutables"></param>
        /// <param name="titleName"></param>
        /// <returns></returns>
        internal static bool ContainsTitle(this CustomExecutables customExecutables, string titleName)
        {
            if (customExecutables == null || customExecutables.List == null || string.IsNullOrWhiteSpace(titleName))
            {
                return false;
            }

            foreach (var custom in customExecutables.List)
            {
                if (custom.Value.Binary == CustomExecutables.NormalizePath(titleName))
                {
                    return true;
                }
            }

            return false;
        }
    }

    class MetaIniInfo
    {
        INIFile _ini;
        public MetaIniParams Get(string moddir)
        {
            string metaPath = Path.Combine(moddir, "meta.ini");
            if (!File.Exists(metaPath))
            {
                return null;
            }

            _ini = ManageIni.GetINIFile(metaPath);

            return new MetaIniParams(
                _ini.GetKey("General", "gameName"),
                _ini.GetKey("General", "category").Trim('\"'),
                _ini.GetKey("General", "version"),
                _ini.GetKey("General", "comments"),
                _ini.GetKey("General", "notes"),
                _ini.GetKey("General", "url")
                );
        }

        public void Set(string moddir, string categoryNames = "", string version = "", string comments = "", string notes = "")
        {
            if (!Directory.Exists(moddir))
            {
                return;
            }

            string metaPath = Path.Combine(moddir, "meta.ini");
            if (!File.Exists(metaPath))
            {
                File.WriteAllText(metaPath, ManageSettings.GetDefaultMetaIni());
            }

            INIFile ini = ManageIni.GetINIFile(metaPath);

            if (!ini.KeyExists("category", "General") || (categoryNames.Replace(",", "").Length > 0 && ini.GetKey("General", "category").Trim('\"').Length == 0))
            {
                ini.SetKey("General", "category", "\"" + GetCategoryIndexForTheName(categoryNames) + "\"");
            }

            if (version.Length > 0)
            {
                ini.SetKey("General", "version", version);
            }

            ini.SetKey("General", "gameName", ManageSettings.GetMoCurrentGameName());

            if (comments.Length > 0)
            {
                ini.SetKey("General", "comments", comments);
            }

            if (notes.Length > 0)
            {
                ini.SetKey("General", "notes", "\"" + notes.Replace(Environment.NewLine, "<br>") + "\"");
            }

            ini.SetKey("General", "validated", "true");
        }

        public class MetaIniParams
        {
            public string GameName;
            public string Category;
            public string Version;
            public string Comments;
            public string Notes;
            public string Url;

            public MetaIniParams(string gameName, string category, string version, string comments, string notes, string url)
            {
                GameName = gameName;
                Category = category;
                Version = version;
                Comments = comments;
                Notes = notes;
                Url = url;
            }
        }
    }

    class CategoriesInfo
    {
        public List<CategoryLineInfo> Records = new List<CategoryLineInfo>();

        string _categoriesFilePath;
        int RecordsInitCount;

        public CategoriesInfo(string categoriesFilePath = null)
        {
            Get(categoriesFilePath ?? ManageSettings.GetMOcategoriesPathForSelectedGame());
        }

        public string Add(string categoryName, bool doSave = false)
        {
            Records.Add(new CategoryLineInfo(new[] { Records.Count + 1 + "", categoryName, "", "0" }));

            return Records.Count + 1 + "";
        }

        private void Get(string categoriesFilePath)
        {
            if (!File.Exists(categoriesFilePath))
            {
                return;
            }

            _categoriesFilePath = categoriesFilePath;

            using (StreamReader reader = new StreamReader(categoriesFilePath))
            {
                string[] lineData;
                while (!reader.EndOfStream)
                {
                    lineData = reader.ReadLine().Split('|');
                    if (lineData == null || lineData.Length != 4)
                    {
                        continue;
                    }

                    Records.Add(new CategoryLineInfo(lineData));
                }
            }

            RecordsInitCount = Records.Count;
        }

        internal void Save()
        {
            if (RecordsInitCount == Records.Count)
            {
                // not changed
                return;
            }

            using (StreamWriter writer = new StreamWriter(_categoriesFilePath))
            {
                foreach (var record in Records)
                {
                    writer.WriteLine(record.ID + "|" + record.Name + "|" + record.NexisID + "|" + record.ParentID);
                }
            }
        }

        public class CategoryLineInfo
        {
            public string ID;
            public string Name;
            public string NexisID = "";
            public string ParentID = "0";

            /// <summary>
            /// ID + Name + NexisID + ParentID
            /// </summary>
            /// <param name="categoryInfo"></param>
            public CategoryLineInfo(string[] categoryInfo)
            {
                ID = categoryInfo[0];
                Name = categoryInfo[1];
                NexisID = categoryInfo[2];
                ParentID = categoryInfo[3];
            }
        }
    }
}
