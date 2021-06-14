using AI_Helper.Manage;
using AIHelper.SharedData;
using INIFileMan;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageMO
    {

        internal static string MOremoveByteArray(string mOSelectedProfileDirPath)
        {
            if (mOSelectedProfileDirPath.StartsWith("@ByteArray("))
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

        public static void RedefineGameMOData()
        {
            //MOini
            ManageSymLinks.ReCreateFileLinkWhenNotValid(ManageSettings.GetMOiniPath(), ManageSettings.GetMOiniPathForSelectedGame(), true);
            //Categories
            ManageSymLinks.ReCreateFileLinkWhenNotValid(ManageSettings.GetMOcategoriesPath(), ManageSettings.GetMOcategoriesPathForSelectedGame(), true);
        }

        internal static string GetMOVersion()
        {
            var exeversion = System.Diagnostics.FileVersionInfo.GetVersionInfo(ManageSettings.GetMOexePath());
            return exeversion.FileVersion;
        }

        public static void SetModOrganizerINISettingsForTheGame()
        {
            RedefineGameMOData();

            //менять настройки МО только когда игра меняется
            if (!Properties.Settings.Default.CurrentGameIsChanging && Path.GetDirectoryName(ManageSettings.GetMOdirPath()) != Properties.Settings.Default.ApplicationStartupPath)
            {
                return;
            }

            INIFile INI = new INIFile(ManageSettings.GetModOrganizerINIpath());

            SetCommonIniValues(INI);

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
        internal static string MakeMOProfileModlistFileBuckup(string suffix = "")
        {
            var modlistPath = ManageSettings.CurrentMOProfileModlistPath();
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
            foreach (var file in new DirectoryInfo(ManageSettings.GetMOSelectedProfileDirPath()).EnumerateFiles("modlist.txt.*" + suffix))
            {
                if (last == null || last.LastWriteTime < file.LastWriteTime)
                {
                    last = file;
                }
            }
            return last != null ? last.FullName : null;
        }

        /// <summary>
        /// changes incorrect relative paths "../Data" fo custom executable for absolute Data path of currents game
        /// </summary>
        /// <param name="INI"></param>
        internal static void FixCustomExecutablesIniValues(INIFile INI = null)
        {
            if (INI == null)
            {
                INI = new INIFile(ManageSettings.GetModOrganizerINIpath());
            }

            var customExecutables = INI.ReadSectionKeyValuePairsToDictionary("customExecutables");

            var newcustomExecutables = new Dictionary<string, string>();

            var changed = false;
            foreach (var record in customExecutables)
            {
                if ((record.Key.EndsWith(@"\binary", StringComparison.OrdinalIgnoreCase)//read bynary path
                   || record.Key.EndsWith(@"\workingDirectory", StringComparison.OrdinalIgnoreCase))//read \workingDirectory path
                    && record.Value.StartsWith("..", StringComparison.InvariantCulture))
                {
                    try
                    {
                        //replace .. to absolute path of current game directory
                        var targetcorrectedrelative = record.Value
                                .Remove(0, 2).Insert(0, ManageSettings.GetCurrentGamePath());

                        //replace other slashes
                        var targetcorrectedabsolute = Path.GetFullPath(targetcorrectedrelative);
                        var targetcorrectedabsolutefixedslash = targetcorrectedabsolute.Replace(@"\\", "/").Replace(@"\", "/");//replace \ to /;

                        //add absolute path for current game's data path
                        newcustomExecutables.Add(record.Key, targetcorrectedabsolutefixedslash);
                        changed = true;
                    }
                    catch
                    {
                        ManageLogs.Log("FixCustomExecutablesIniValues:Error while path fix.\r\nKey=" + record.Key + "\r\nPath=" + record.Value);
                        newcustomExecutables.Add(record.Key, record.Value);
                    }
                }
                else
                {
                    newcustomExecutables.Add(record.Key, record.Value);
                }
            }

            if (changed)//write only if changed
            {
                foreach (var record in newcustomExecutables)
                {
                    INI.WriteINI("customExecutables", record.Key, record.Value, false);
                }
                INI.SaveINI();
            }
        }

        private static void SetCustomExecutablesIniValues(INIFile INI)
        {
            if (Properties.Settings.Default.SetModOrganizerINISettingsForTheGame)
            {
                return;
            }

            Properties.Settings.Default.SetModOrganizerINISettingsForTheGame = true;

            string[,] IniValues = new string[,] { };

            if (!ManageSettings.MOIsNew)
            {
                IniValues = new string[,]
                    {
                    //customExecutables
                    {
                        ManageSettings.GetCurrentGameEXEName()
                        ,
                        @"1\title"
                    }
                ,
                    {
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(), ManageSettings.GetCurrentGameEXEName() + ".exe")
                        ,
                        @"1\binary"
                    }
                ,
                    {
                        ManageSettings.GetCurrentGameDataPath()
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
                        ManageSettings.GetCurrentGameDataPath()
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
                        Path.Combine(ManageSettings.GetCurrentGameDataPath(),ManageSettings.GetStudioEXEName()+".exe")
                        ,
                        @"3\binary"
                    }
                ,
                    {
                        ManageSettings.GetCurrentGameDataPath()
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
            }


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
                ,
                    {
                        "hide"
                    ,
                        "b"
                    }
            };

            int resultindex = 0;//конечное количество добавленных exe
            if (!ManageSettings.MOIsNew)
            {
                //заполнить словарь значениями массива строк
                resultindex = 1;//конечное количество добавленных exe
                int IniValuesLength = IniValues.Length / 2;//длина массива, деленая на 2 т.к. каждый элемент состоит из двух
                int exclude = 0;//будет равен индексу, который надо пропустить
                int skippedCnt = 0;//счет количества пропущенных, нужно для правильного подсчета resultindex
                for (int i = 0; i < IniValuesLength; i++)
                {
                    var curindex = int.Parse(IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0], CultureInfo.InvariantCulture);
                    var parameterName = IniValues[i, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[1];

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

                    var ResultparameterName = resultindex + @"\" + parameterName;

                    if (!IniValuesDict.ContainsKey(ResultparameterName))
                    {
                        IniValuesDict.Add(ResultparameterName, IniValues[i, 0]);
                    }
                }
            }

            var ExecutablesCount = resultindex;
            //try
            //{
            //    ExecutablesCount = int.Parse(IniValues[IniValuesLength - 1, 1].Split(new string[1] { @"\" }, StringSplitOptions.None)[0]);
            //}
            //catch
            //{
            //    return;//если не число, выйти
            //}

            AddCustomExecutables(INI, out Dictionary<string, string> customExecutables, ref ExecutablesCount);

            if (customExecutables.Count > 0)
            {
                //очистка секции
                INI.ClearSection("customExecutables", false);
                foreach (var pair in customExecutables)
                {
                    if (!IniValuesDict.ContainsKey(pair.Key))
                    {
                        IniValuesDict.Add(pair.Key, pair.Value);
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
                        string subquote = key.EndsWith(@"\arguments", StringComparison.InvariantCulture) && keyExists ? "\\\"" : string.Empty;
                        if (keyExists)
                        {
                            IniValue = subquote + IniValuesDict[key].Replace(@"\", key.EndsWith(@"\arguments", StringComparison.InvariantCulture) ? @"\\" : "/") + subquote;
                        }
                        else
                        {
                            IniValue = subquote + (iniParameters[i, 1].EndsWith("b", StringComparison.InvariantCulture) ? "false" : string.Empty) + subquote;
                        }

                        INI.WriteINI("customExecutables", key, IniValue, false);
                    }
                    cnt++;
                }

                //Hardcoded exe Game exe and Explorer++
                //ExecutablesCount += 2;

                INI.WriteINI("customExecutables", "size", ExecutablesCount.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                INI.SaveINI(true, true);
            }

            Properties.Settings.Default.SetModOrganizerINISettingsForTheGame = false;
        }

        /// <summary>
        /// true if MO have base plugin
        /// </summary>
        /// <returns></returns>
        internal static bool IsMO23ORNever()
        {
            var ver = GetMOVersion();
            return ver != null && ver.Length > 2 && ver[0] == '2' && int.Parse(ver[2].ToString(), CultureInfo.InvariantCulture) > 2;
        }

        private static void AddCustomExecutables(INIFile INI, out Dictionary<string, string> customExecutables, ref int ExecutablesCount)
        {
            customExecutables = new Dictionary<string, string>();

            var exists = INI.ReadSectionValuesToArray("customExecutables");

            var exeEqual = 0;
            var OneexeNotEqual = false;
            var executablesCount = ExecutablesCount;
            Dictionary<string, string> customs = new Dictionary<string, string>();

            if (!ManageSettings.MOIsNew)
            {
                var CurrentGame = Data.CurrentGame;
                if (CurrentGame.GetGameEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameEXENameX32() + ".exe")))
                {
                    executablesCount++;
                    exeEqual++;
                    customs.Add(executablesCount + @"\title", CurrentGame.GetGameEXENameX32());
                    customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameEXENameX32() + ".exe"));
                    customs.Add(executablesCount + @"\workingDirectory", ManageSettings.GetCurrentGameDataPath());
                }
                if (CurrentGame.GetGameStudioEXENameX32().Length > 0 && File.Exists(Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe")))
                {
                    executablesCount++;
                    customs.Add(executablesCount + @"\title", CurrentGame.GetGameStudioEXENameX32());
                    customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetCurrentGameDataPath(), CurrentGame.GetGameStudioEXENameX32() + ".exe"));
                    customs.Add(executablesCount + @"\workingDirectory", ManageSettings.GetCurrentGameDataPath());
                }
            }
            else
            {
                foreach (var value in ManagePython.GetExecutableInfosFromPyPlugin())
                {
                    if (string.IsNullOrWhiteSpace(value.Key) || !File.Exists(value.Value))
                    {
                        continue;
                    }

                    executablesCount++;
                    customs.Add(executablesCount + @"\title", value.Key);
                    customs.Add(executablesCount + @"\binary", value.Value);
                    customs.Add(executablesCount + @"\arguments", string.Empty);
                    customs.Add(executablesCount + @"\workingDirectory", Path.GetDirectoryName(value.Value));
                    customs.Add(executablesCount + @"\ownicon", "true");
                }
            }

            string[] pathExclusions =
                {
                "BepInEx" + Path.DirectorySeparatorChar + "plugins",
                //"Lec.ExtProtocol",
                //"ezTransXP.ExtProtocol",
                ".ExtProtocol",
                //"Common.ExtProtocol.Executor",
                "UnityCrashHandler64",
                Path.DirectorySeparatorChar + "IPA",
                "WideSliderPatch"
                };

            {
                //Добавление exe из Data
                //Parallel.ForEach(Directory.EnumerateFileSystemEntries(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories),
                //    exePath =>
                //    {
                //        try
                //        {
                //            if (exeEqual > 9)
                //            {
                //                return;
                //            }

                //            var exeName = Path.GetFileNameWithoutExtension(exePath);
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

                //    });
            }

            foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetCurrentGameDataPath(), "*.exe", SearchOption.AllDirectories))
            {
                try
                {
                    if (exeEqual > 9)
                    {
                        return;
                    }

                    var exeName = Path.GetFileNameWithoutExtension(exePath);
                    if (exeName.Length > 0 && !customs.Values.Contains(exeName) && Path.GetDirectoryName(exePath) != ManageSettings.GetCurrentGameDataPath() && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
                    {
                        executablesCount++;
                        if (!OneexeNotEqual)
                        {
                            if (exists.Contains(exePath))
                            {
                                exeEqual++;
                            }
                            else
                            {
                                exeEqual = 0;
                                OneexeNotEqual = true;
                            }
                        }

                        customs.Add(executablesCount + @"\title", exeName);
                        customs.Add(executablesCount + @"\binary", exePath);
                    }
                }
                catch { }
            }

            {
                //foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetDataPath(), "*.exe", SearchOption.AllDirectories))
                //{
                //    string exeName = Path.GetFileNameWithoutExtension(exePath);
                //    if (exeName.Length > 0 && !customExecutables.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
                //    {
                //        ExecutablesCount++;
                //        customExecutables.Add(ExecutablesCount + @"\title", exeName);
                //        customExecutables.Add(ExecutablesCount + @"\binary", exePath);
                //    }
                //}
                //Добавление exe из Mods
                //Parallel.ForEach(Directory.EnumerateFileSystemEntries(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories),
                //    exePath =>
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

                //    });
                //foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetModsPath(), "*.exe", SearchOption.AllDirectories))
                //{
                //    string exeName = Path.GetFileNameWithoutExtension(exePath);
                //    if (Path.GetFileNameWithoutExtension(exePath).Length > 0 && !customExecutables.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
                //    {
                //        ExecutablesCount++;
                //        customExecutables.Add(ExecutablesCount + @"\title", exeName);
                //        customExecutables.Add(ExecutablesCount + @"\binary", exePath);
                //    }
                //}
            }

            foreach (var exePath in Directory.EnumerateFiles(ManageSettings.GetCurrentGameModsPath(), "*.exe", SearchOption.AllDirectories))
            {
                try
                {
                    if (exeEqual > 9)
                    {
                        return;
                    }

                    string exeName = Path.GetFileNameWithoutExtension(exePath);
                    if (exeName.Length > 0 && !customs.Values.Contains(exeName) && !ManageStrings.IsStringAContainsAnyStringFromStringArray(exePath, pathExclusions, true))
                    {
                        executablesCount++;
                        if (!OneexeNotEqual)
                        {
                            if (exists.Contains(exePath))
                            {
                                exeEqual++;
                            }
                            else
                            {
                                exeEqual = 0;
                                OneexeNotEqual = true;
                            }
                        }

                        customs.Add(executablesCount + @"\title", exeName);
                        customs.Add(executablesCount + @"\binary", exePath);
                    }
                }
                catch { }
            }

            //добавление hardcoded exe
            //executablesCount++;
            //customs.Add(executablesCount + @"\title", "Skyrim");
            //customs.Add(executablesCount + @"\binary", Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "TESV.exe"));
            //customs.Add(executablesCount + @"\workingDirectory", Properties.Settings.Default.ApplicationStartupPath);
            //customs.Add(executablesCount + @"\ownicon", "true");
            //executablesCount++;
            //customs.Add(executablesCount + @"\title", "Explore Virtual Folder");
            //customs.Add(executablesCount + @"\binary", Path.Combine(ManageSettings.GetMOdirPath(), "explorer++", "Explorer++.exe"));
            //customs.Add(executablesCount + @"\workingDirectory", Path.Combine(ManageSettings.GetMOdirPath(), "explorer++"));
            //customs.Add(executablesCount + @"\arguments", ManageSettings.GetCurrentGameDataPath());
            //customs.Add(executablesCount + @"\ownicon", "true");

            customExecutables = customs;
            ExecutablesCount = executablesCount;
        }

        private static void SetCommonIniValues(INIFile INI)
        {
            string[,] IniValues =
                {
                    //General
                    {
                        "@ByteArray("+ManageSettings.GetCurrentGamePath()+")"
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
                //        "@ByteArray("+"selected_profile"+")"
                //    }


                //,
                //    {
                //        "1"
                //        ,
                //        "General"
                //        ,
                //        @"selected_executable"
                //    }


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
                        ManageSettings.GetCurrentGameModsPath()
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
                string subquote = IniValues[i, 2].EndsWith(@"\arguments", StringComparison.InvariantCulture) ? "\\\"" : string.Empty;
                string IniValue = subquote + IniValues[i, 0].Replace(@"\", @"\\") + subquote;
                //if (INIManage.GetINIValueIfExist(SettingsManage.GetModOrganizerINIpath(), IniValues[i, 2], IniValues[i, 1]) != IniValue)
                //{
                //    INI.WriteINI(IniValues[i, 1], IniValues[i, 2], IniValue);
                //}
                INI.WriteINI(IniValues[i, 1], IniValues[i, 2], IniValue, false);
            }

            INI.SaveINI();
        }

        internal static Dictionary<string, string> GetMOcustomExecutablesList()
        {
            var INI = new INIFile(ManageSettings.GetMOiniPath());
            var customs = INI.ReadSectionKeyValuePairsToDictionary("customExecutables");
            var retDict = new Dictionary<string, string>();
            foreach (var pair in customs)
            {
                if (pair.Key.EndsWith(@"\title", StringComparison.InvariantCulture))
                {
                    retDict.Add(pair.Value, customs[pair.Key.Split('\\')[0] + @"\binary"]);
                }
            }
            return retDict;
        }

        internal static string GetMOcustomExecutableTitleByExeName(string exename)
        {
            var customs = new INIFile(ManageSettings.GetMOiniPath()).ReadSectionKeyValuePairsToDictionary("customExecutables");
            foreach (var pair in customs)
            {
                if (pair.Key.EndsWith(@"\binary", StringComparison.InvariantCulture))
                    if (Path.GetFileNameWithoutExtension(pair.Value) == exename)
                        if (File.Exists(pair.Value))
                        {
                            return customs[pair.Key.Split('\\')[0] + @"\title"];
                        }
            }
            return exename;
        }

        /// <summary>
        /// inserts in MO.ini new custom executable
        /// required exeParams 0 is exe title, required exeParams 1 is exe bynary path
        /// optional exeParams 2 is arguments, optional exeParams 4 is working directory
        /// </summary>
        /// <param name="exeParams"></param>
        internal static void InsertCustomExecutable(string[] exeParams)
        {
            var INI = new INIFile(ManageSettings.GetMOiniPath());
            var customs = INI.ReadSectionKeyValuePairsToDictionary("customExecutables");

            var newind = GetMOiniCustomExecutablesCount(customs) + 1; //get new custom executables index

            //add parameterso of new executable
            customs.Add(newind + @"\title", exeParams[0]);
            customs.Add(newind + @"\binary", exeParams[1].Replace(@"\\", "/").Replace(@"\", "/"));
            customs.Add(newind + @"\arguments", exeParams.Length > 2 ? @"\""" + exeParams[2].Replace(@"\", @"\\").Replace("/", @"\\") + @"\""" : string.Empty);
            customs.Add(newind + @"\workingDirectory", exeParams.Length > 3 ? exeParams[3] : Path.GetDirectoryName(exeParams[0]).Replace(@"\\", "/").Replace(@"\", "/"));
            customs.Add(newind + @"\ownicon", "true");

            //update parameters size
            customs.Remove("size");
            customs["size"] = newind + "";

            //write customs
            foreach (var record in customs)
            {
                INI.WriteINI("customExecutables", record.Key, record.Value, false);
            }
            INI.SaveINI();
        }

        /// <summary>
        /// get MO custom executables count
        /// </summary>
        /// <param name="customs"></param>
        /// <returns></returns>
        internal static int GetMOiniCustomExecutablesCount(Dictionary<string, string> customs = null)
        {
            customs = customs ?? new INIFile(ManageSettings.GetMOiniPath()).ReadSectionKeyValuePairsToDictionary("customExecutables");

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
            var customs = new INIFile(ManageSettings.GetMOiniPath()).ReadSectionKeyValuePairsToDictionary("customExecutables");
            if (customs != null)
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

        public static void DeactivateMod(string modname)
        {
            ActivateDeactivateInsertMod(modname, false);
        }

        public static void InsertMod(string modname, bool Activate = true, string modAfterWhichInsert = "", bool PlaceAfter = true)
        {
            ActivateDeactivateInsertMod(modname, Activate, modAfterWhichInsert, PlaceAfter);
        }

        public static void ActivateDeactivateInsertMod(string modname, bool Activate = true, string modAfterWhichInsert = "", bool PlaceAfter = true)
        {
            if (modname.Length > 0)
            {
                string currentMOprofile = ManageSettings.GetMOSelectedProfileDirName();

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

        /// <summary>
        /// return list of mo names in active profile folder
        /// </summary>
        /// <param name="OnlyEnabled"></param>
        /// <returns></returns>
        public static string[] GetModNamesListFromActiveMOProfile(bool OnlyEnabled = true)
        {
            string currentMOprofile = ManageSettings.GetMOSelectedProfileDirName();

            if (currentMOprofile.Length > 0)
            {
                string profilemodlistpath = Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile, "modlist.txt");

                //фикс на случай несовпадения выбранной игры и профиля в MO ini
                if (!File.Exists(profilemodlistpath))
                {
                    ManageMO.RedefineGameMOData();
                    currentMOprofile = ReGetcurrentMOprofile(currentMOprofile);

                    profilemodlistpath = Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile, "modlist.txt");
                }

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

        private static string ReGetcurrentMOprofile(string currentMOprofile)
        {
            while (!Directory.Exists(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", currentMOprofile)))
            {
                if (Directory.Exists(Path.Combine(ManageSettings.GetCurrentGamePath(), "MO", "profiles", ManageSettings.GetCurrentGameEXEName())))
                {
                    return ManageSettings.GetCurrentGameEXEName();
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

        public static string GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(string[] pathInMods, bool[] IsDir)
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
                            if ((IsDir[d] && Directory.Exists(pathCandidate)) || (!IsDir[d] && File.Exists(pathCandidate)))
                            {
                                return pathCandidate;
                            }
                            else
                            {
                                path = GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(pathCandidate, IsDir[d]);
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

        public static string GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(string pathInMods, bool IsDir = false, bool OnlyEnabled = true)
        {
            if (string.IsNullOrWhiteSpace(pathInMods))
            {
                return pathInMods;
            }

            try
            {
                string ModsOverwrite = pathInMods.Contains(ManageSettings.GetCurrentGameMOOverwritePath()) ? ManageSettings.GetCurrentGameMOOverwritePath() : ManageSettings.GetCurrentGameModsPath();

                //искать путь только для ссылки в Mods или в Data
                if (!ManageStrings.IsStringAContainsStringB(pathInMods, ModsOverwrite) && !ManageStrings.IsStringAContainsStringB(pathInMods, ManageSettings.GetCurrentGameDataPath()))
                    return pathInMods;

                //отсеивание первого элемента с именем мода
                //string subpath = string.Empty;

                string[] pathInModsElements = pathInMods
                    .Replace(ModsOverwrite, string.Empty)
                    .Split(Path.DirectorySeparatorChar)
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                //для мода в mods пропустить имя этого мода
                if (ModsOverwrite == ManageSettings.GetCurrentGameModsPath())
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

                if (!Properties.Settings.Default.MOmode)
                {
                    return Path.Combine(ManageSettings.GetCurrentGameDataPath(), subpath);
                }

                //check in Overwrite 1st
                string overwritePath = ManageSettings.GetCurrentGameMOOverwritePath() + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + subpath;
                if (IsDir)
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
                string ModsPath = ManageSettings.GetCurrentGameModsPath();
                foreach (var modName in GetModNamesListFromActiveMOProfile(OnlyEnabled))
                {
                    string possiblePath = Path.Combine(ModsPath, modName) + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + subpath;
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
            if (Properties.Settings.Default.MOmode)
            {
                string currentMOprofile = ManageSettings.GetMOSelectedProfileDirName();

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
                            if (lines[i].StartsWith("+", StringComparison.InvariantCulture))
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
                return Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "setup.xml");
            }
        }

        public static string GetMetaParameterValue(string MetaFilePath, string NeededValue)
        {
            return ManageINI.GetINIValueIfExist(MetaFilePath, NeededValue);

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

        /// <summary>
        /// для МО до версии 2.3 -создание болванки, для 2.3 и новее - удаление, если есть
        /// </summary>
        public static void DummyFiles()
        {
            if (ManageSettings.MOIsNew)
            {
                if (File.Exists(ManageSettings.GetDummyFilePath()))
                {
                    File.Delete(ManageSettings.GetDummyFilePath());
                }
                if (File.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "")))
                {
                    File.Delete(ManageSettings.GetDummyFilePath());
                }
            }
            else
            {
                //Create dummy file and add hidden attribute
                if (!File.Exists(ManageSettings.GetDummyFilePath()))
                {
                    File.Copy(ManageSettings.GetDummyFileRESPath(), ManageSettings.GetDummyFilePath());
                    //new FileInfo(ManageSettings.GetDummyFilePath()).Create().Close();
                    //File.WriteAllText(ManageSettings.GetDummyFilePath(), "dummy file need to execute mod organizer");
                    ManageFilesFolders.HideFileFolder(ManageSettings.GetDummyFilePath(), true);
                }
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

        internal static void MOINIFixes()
        {
            if (!File.Exists(ManageSettings.GetModOrganizerINIpath())) return;

            var INI = new INIFile(ManageSettings.GetModOrganizerINIpath());
            if (INI == null) return;

            string gameName;
            //updated game name
            if (string.IsNullOrWhiteSpace(gameName = INI.ReadINI("General", "gameName")) || gameName != ManageSettings.GETMOCurrentGameName())
            {
                INI.WriteINI("General", "gameName", ManageSettings.GETMOCurrentGameName(), false);
            }

            //clear pluginBlacklist section of MO ini to prevent plugin_python.dll exist there
            if (INI.SectionExistsAndNotEmpty("pluginBlacklist"))
            {
                INI.DeleteSection("pluginBlacklist", false);
            }

            //Set selected_executable number to game exe
            var Customs = INI.ReadSectionKeyValuePairsToDictionary("customExecutables");
            if (Customs != null)
            {
                foreach (var INIKeyValue in Customs)
                {
                    if (!INIKeyValue.Key.EndsWith("binary", StringComparison.InvariantCulture))
                    {
                        continue;
                    }

                    if (Path.GetFileNameWithoutExtension(INIKeyValue.Value) == ManageSettings.GetCurrentGameEXEName())
                    {
                        var index = INIKeyValue.Key.Split('\\')[0];
                        INI.WriteINI("General", "selected_executable", index, false);
                        INI.WriteINI("Widgets", "MainWindow_executablesListBox_index", index, false);
                        break;
                    }
                }
            }

            INI.WriteINI("PluginPersistance", @"Python%20Proxy\tryInit", "false");
        }
    }
}
