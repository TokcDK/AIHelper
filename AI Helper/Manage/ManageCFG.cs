using AIHelper.Utils;
using System.IO;

namespace AIHelper.Manage
{
    internal static class ManageCfg
    {
        public static string GetCfgValueIfExist(string cfgPath, string key, string section, string defaultValue = "")
        {
            if (File.Exists(cfgPath))
            {
                CfgFiles cfg = new CfgFiles(cfgPath);
                if (cfg.KeyExists(key, section)) return cfg.ReadCfg(section, key);
            }
            return defaultValue;
        }
        public static bool WriteCfgValue(string cfgPath, string section, string key, string value, bool doSaveIni = true)
        {
            if (File.Exists(cfgPath))
            {
                (new CfgFiles(cfgPath)).WriteCfg(section, key, value, doSaveIni);
                return true;
            }
            return false;
        }
    }
}
