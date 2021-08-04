using INIFileMan;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AIHelper.Manage
{
    internal static class ManageIni
    {
        public static string GetIniValueIfExist(string iniPath, string key, string section = null, string defaultValue = "")
        {
            if (!File.Exists(iniPath))
            {
                ManageModOrganizer.RedefineGameMoData();
            }

            INIFile ini = new INIFile(iniPath);
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
            (new INIFile(iniPath)).SetKey(section, key, value, doSaveIni);
            return true;
            //return false;
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="moModlistPath"></param>
        /// <param name="line"></param>
        /// <param name="position"></param>
        public static void InsertLineInFile(string moModlistPath, string line, int position = 1, string insertWithThisMod = "", bool placeAfter = true)
        {
            if (moModlistPath.Length > 0 && File.Exists(moModlistPath) && line.Length > 0)
            {
                string[] fileLines = File.ReadAllLines(moModlistPath);
                bool isEnabled;
                if (!(isEnabled = fileLines.Contains("+" + line.Remove(0, 1))) && !fileLines.Contains("-" + line.Remove(0, 1)))
                {
                    int fileLinesLength = fileLines.Length;
                    bool insertWithMod = insertWithThisMod.Length > 0;
                    position = insertWithMod ? fileLinesLength : position;
                    using (StreamWriter writer = new StreamWriter(moModlistPath))
                    {
                        for (int lineNumber = 0; lineNumber < position; lineNumber++)
                        {
                            if (insertWithMod && fileLines[lineNumber].Length > 0 && string.Compare(fileLines[lineNumber].Remove(0, 1), insertWithThisMod, true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (placeAfter)
                                {
                                    position = lineNumber;
                                }
                                else
                                {
                                    writer.WriteLine(fileLines[lineNumber]);
                                    position = lineNumber + 1;
                                }
                                break;

                            }
                            else
                            {
                                writer.WriteLine(fileLines[lineNumber]);
                            }
                        }

                        writer.WriteLine(line);

                        if (position < fileLinesLength)
                        {
                            for (int lineNumber = position; lineNumber < fileLinesLength; lineNumber++)
                            {
                                writer.WriteLine(fileLines[lineNumber]);
                            }
                        }
                    }
                }
                else
                {
                    var prefix = "-";
                    if (isEnabled)
                    {
                        prefix = "+";
                    }
                    fileLines = fileLines.Replace(prefix + line.Remove(0, 1), line);
                    File.WriteAllLines(moModlistPath, fileLines);
                }
            }
        }
    }
}
