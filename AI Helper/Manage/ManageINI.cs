using INIFileMan;
using NLog;
using System.IO;

namespace AIHelper.Manage
{
    internal static class ManageIni
    {
        static Logger _log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// init ini file and set required settings for it
        /// </summary>
        /// <param name="iniPath"></param>
        /// <returns></returns>
        public static INIFile GetINIFile(string iniPath, bool createIfNoIni = true)
        {
            var ini = new INIFile(iniPath);
            if (ini.Configuration == null)
            {
                if (createIfNoIni)
                {
                    File.WriteAllText(iniPath, string.Empty);
                    ini = new INIFile(iniPath);
                }
                else
                {
                    _log.Debug("GetINIFile error. ini is null. iniPath=" + iniPath);
                }
            }
            else
            {
                ini.Configuration.AssigmentSpacer = "";
            }

            return ini;
        }

        public static string GetIniValueIfExist(string iniPath, string key, string section = null, string defaultValue = "")
        {
            if (!File.Exists(iniPath))
            {
                ManageModOrganizer.RedefineGameMoData();
            }

            INIFile ini = GetINIFile(iniPath);
            if (ini.KeyExists(key, section))
            {
                return ini.GetKey(section, key);
            }
            return defaultValue;
        }
        public static bool WriteIniValue(string iniPath, string section, string key, string value, bool doSaveIni = true)
        {
            if (!File.Exists(iniPath))
            {
                ManageModOrganizer.RedefineGameMoData();
            }
            GetINIFile(iniPath).SetKey(section, key, value, doSaveIni);
            return true;
            //return false;
        }
    }
}
