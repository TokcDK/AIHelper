using AIHelper.Utils;
using System.IO;

namespace AIHelper.Manage
{
    internal static class ManageCFG
    {
        public static string GetCFGValueIfExist(string CFGPath, string Key, string Section, string defaultValue = "")
        {
            if (File.Exists(CFGPath))
            {
                CFGFiles CFG = new CFGFiles(CFGPath);
                if (CFG.KeyExists(Key, Section))
                {
                    return CFG.ReadCFG(Section, Key);
                }
            }
            return defaultValue;
        }
        public static bool WriteCFGValue(string CFGPath, string Section, string Key, string Value, bool DOSaveINI = true)
        {
            if (File.Exists(CFGPath))
            {
                (new CFGFiles(CFGPath)).WriteCFG(Section, Key, Value, DOSaveINI);
                return true;
            }

            return false;
        }
    }
}
