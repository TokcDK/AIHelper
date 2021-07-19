using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage
{
    class ManagePython
    {
        internal static Dictionary<string, string> GetExecutableInfosFromPyPlugin()
        {
            string line;
            Dictionary<string, string> executables = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(ManageSettings.GetCurrentGameMOGamePyPluginPath()))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine().Trim();
                    if (line.Contains("mobase.ExecutableInfo("))
                    {
                        var title = sr.ReadLine().Trim().Trim(',');
                        title = title.Trim().Trim(',');
                        var binary = sr.ReadLine();
                        binary = binary.Trim().Trim(',', ')', '"');
                        binary = binary.Replace('/', Path.DirectorySeparatorChar);
                        binary = binary
                            .Replace("QFileInfo(", string.Empty)
                            .Replace("self.gameDirectory(), \"", Path.GetDirectoryName(ManageSettings.GetCurrentGameDataPath()) + Path.DirectorySeparatorChar);
                        executables.Add(title, binary);
                    }
                }
            }

            return executables;
        }
    }
}
