//https://github.com/cemdervis/SharpConfig
using SharpConfig;

namespace AIHelper.Utils
{
    public class CFGFiles
    {
        readonly Configuration config;
        readonly string filePath;

        public CFGFiles(string CFGPath)
        {
            filePath = CFGPath;
            config = Configuration.LoadFromFile(CFGPath);
            Configuration.SpaceBetweenEquals = false;
        }

        public string ReadCFG(string Section, string Key)
        {
            return config[Section][Key].StringValue;
        }

        public void WriteCFG(string Section, string Key, string Value, bool DoSaveCFG = true)
        {
            config[Section][Key].StringValue = Value;

            if (DoSaveCFG)
            {
                config.SaveToFile(filePath);
            }
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return config[Section].Contains(Key);
        }
    }
}
