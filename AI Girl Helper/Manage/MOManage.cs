using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Girl_Helper.Manage
{
    class MOManage
    {
        public static void SetModOrganizerINISettingsForTheGame()
        {
            Utils.IniFile INI = new Utils.IniFile(Properties.Settings.Default.ModOrganizerINIpath);

            //General
            string IniValue = SettingsManage.GetCurrentGamePath().Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, "gamePath", "General") != IniValue)
            {
                INI.WriteINI("General", "gamePath", IniValue);
            }
            //customExecutables
            IniValue = SettingsManage.GetCurrentGameName().Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"1\title", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"1\title", IniValue);
            }
            IniValue = Path.Combine(Properties.Settings.Default.DataPath, SettingsManage.GetCurrentGameName() + ".exe").Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"1\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"1\binary", IniValue);
            }
            IniValue = Properties.Settings.Default.DataPath.Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"1\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"1\workingDirectory", IniValue);
            }
            IniValue = SettingsManage.GetSettingsEXEPath().Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"2\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"2\binary", IniValue);
            }
            IniValue = SettingsManage.GetSettingsEXEPath().Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"2\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"2\binary", IniValue);
            }
            IniValue = Properties.Settings.Default.DataPath.Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"2\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"2\workingDirectory", IniValue);
            }
            IniValue = Path.Combine(Properties.Settings.Default.MODirPath, "explorer++", "Explorer++.exe").Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"3\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"3\binary", IniValue);
            }
            IniValue = "\\\"" + Properties.Settings.Default.DataPath.Replace(@"\", @"\\") + "\\\"";
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"3\arguments", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"3\arguments", IniValue);
            }
            IniValue = Path.Combine(Properties.Settings.Default.MODirPath, "explorer++").Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"3\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"3\workingDirectory", IniValue);
            }
            IniValue = Path.Combine(SettingsManage.GetCurrentGamePath(), "TESV.exe").Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"4\binary", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"4\binary", IniValue);
            }
            IniValue = SettingsManage.GetCurrentGamePath().Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, @"4\workingDirectory", "customExecutables") != IniValue)
            {
                INI.WriteINI("customExecutables", @"4\workingDirectory", IniValue);
            }
            //Settings
            IniValue = Properties.Settings.Default.ModsPath.Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, "mod_directory", "Settings") != IniValue)
            {
                INI.WriteINI("Settings", "mod_directory", IniValue);
            }
            IniValue = Path.Combine(SettingsManage.GetCurrentGamePath(), "Downloads").Replace(@"\", @"\\");
            if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, "download_directory", "Settings") != IniValue)
            {
                INI.WriteINI("Settings", "download_directory", IniValue);
            }

            if (CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                IniValue = "ru";
                if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, "language", "Settings") != IniValue)
                {
                    INI.WriteINI("Settings", "language", IniValue);
                }
            }
            else
            {
                IniValue = "en";
                if (INIManage.GetINIValueIfExist(Properties.Settings.Default.ModOrganizerINIpath, "language", "Settings") != IniValue)
                {
                    INI.WriteINI("Settings", "language", IniValue);
                }
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
                Utils.IniFile INI = new Utils.IniFile(metaPath);

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
                string currentMOprofile = INIManage.GetINIValueIfExist(Path.Combine(Properties.Settings.Default.MODirPath, "ModOrganizer.ini"), "selected_profile", "General");

                if (currentMOprofile.Length == 0)
                {
                }
                else
                {
                    string profilemodlistpath = Path.Combine(Properties.Settings.Default.MODirPath, "profiles", currentMOprofile, "modlist.txt");

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
    }
}
