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

        public static string GetModOrganizerINIpath()
        {
            return Path.Combine(Properties.Settings.Default.MODirPath, "ModOrganizer.ini");
        }
    }
}
