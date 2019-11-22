using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Girl_Helper.Manage
{
    class SettingsManage
    {
        public static string GetSettingsEXEPath()
        {
            return Path.Combine(Properties.Settings.Default.DataPath, "InitSetting.exe");
        }

        public static string GetCurrentGamePath()
        {
            return Properties.Settings.Default.CurrentGamePath;
        }

        public static string GETMOCurrentGameName()
        {
            return "Skyrim";
        }

        public static string GetDummyFile()
        {
            return Path.Combine(GetCurrentGamePath(), "TESV.exe");
        }

        public static string GetCurrentGameName()
        {
            if (File.Exists(Path.Combine(Properties.Settings.Default.DataPath, "AI-SyoujyoTrial.exe")))
            {
                return "AI-SyoujyoTrial";
            }
            else if (File.Exists(Path.Combine(Properties.Settings.Default.DataPath, "AI-Syoujyo.exe")))
            {
                return "AI-Syoujyo";
            }
            return string.Empty;
        }

        public static string GetStudioEXEName()
        {
            return "StudioNEOV2.exe";
        }

        public static string GetINISettingsEXEName()
        {
            return "InitSetting";
        }

        public static string GetAppResDir()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "AI Girl Helper_RES");
        }

        public static string GetModsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Mods");
        }

        public static string GetDownloadsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Downloads");
        }

        public static string GetDataPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Data");
        }

        public static string GetMODirPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO");
        }

        public static string GetMOexePath()
        {
            return Path.Combine(GetMODirPath(), "ModOrganizer.exe");
        }

        public static string GetInstall2MODirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "2MO");
        }

        public static string GetOverwriteFolder()
        {
            return Path.Combine(GetMODirPath(), "overwrite");
        }

        public static string GetOverwriteFolderLink()
        {
            return Path.Combine(GetCurrentGamePath(), "MOUserData");
        }

        public static string GetModOrganizerINIpath()
        {
            return Path.Combine(Properties.Settings.Default.MODirPath, "ModOrganizer.ini");
        }

        public static string GetMOmodeDataFilesBakDirPath()
        {
            return Path.Combine(Properties.Settings.Default.AppResDir, "MOmodeDataFilesBak");
        }

        public static string GetModdedDataFilesListFilePath()
        {
            return Path.Combine(Properties.Settings.Default.AppResDir, "ModdedDataFilesList.txt");
        }

        public static string GetVanillaDataFilesListFilePath()
        {
            return Path.Combine(Properties.Settings.Default.AppResDir, "VanillaDataFilesList.txt");
        }

        public static string GetMOToStandartConvertationOperationsListFilePath()
        {
            return Path.Combine(Properties.Settings.Default.AppResDir, "MOToStandartConvertationOperationsList.txt");
        }
    }
}
