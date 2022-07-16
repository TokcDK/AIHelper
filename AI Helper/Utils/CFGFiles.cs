//https://github.com/cemdervis/SharpConfig
using SharpConfig;

namespace AIHelper.Utils
{
    public class CfgFiles
    {
        readonly Configuration _config;
        readonly string _filePath;

        public CfgFiles(string cfgPath)
        {
            _filePath = cfgPath;
            _config = Configuration.LoadFromFile(cfgPath);
            Configuration.SpaceBetweenEquals = false;
        }

        public string ReadCfg(string section, string key)
        {
            return _config[section][key].StringValue;
        }

        public void WriteCfg(string section, string key, string value, bool doSaveCfg = true)
        {
            _config[section][key].StringValue = value;

            if (doSaveCfg) _config.SaveToFile(_filePath);
        }

        public bool KeyExists(string key, string section = null)
        {
            return _config[section].Contains(key);
        }
    }
}
