using INIFileMan;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AIHelper.Manage
{
    internal static class ManageINI
    {
        public static string GetINIValueIfExist(string INIPath, string Key, string Section = null, string defaultValue = "")
        {
            if (!File.Exists(INIPath))
            {
                ManageMO.RedefineGameMOData();
            }

            INIFile INI = new INIFile(INIPath);
            if (INI.KeyExists(Key, Section))
            {
                return INI.ReadINI(Section, Key);
            }
            return defaultValue;
        }
        public static bool WriteINIValue(string INIPath, string Section, string Key, string Value, bool DOSaveINI = true)
        {
            if (!File.Exists(INIPath))
            {
                ManageMO.RedefineGameMOData();
            }
            (new INIFile(INIPath)).WriteINI(Section, Key, Value, DOSaveINI);
            return true;
            //return false;
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="MOModlistPath"></param>
        /// <param name="line"></param>
        /// <param name="Position"></param>
        public static void InsertLineInFile(string MOModlistPath, string Line, int Position = 1, string InsertWithThisMod = "", bool PlaceAfter = true)
        {
            if (MOModlistPath.Length > 0 && File.Exists(MOModlistPath) && Line.Length > 0)
            {
                string[] FileLines = File.ReadAllLines(MOModlistPath);
                bool IsEnabled;
                if (!(IsEnabled = FileLines.Contains("+" + Line.Remove(0, 1))) && !FileLines.Contains("-" + Line.Remove(0, 1)))
                {
                    int FileLinesLength = FileLines.Length;
                    bool InsertWithMod = InsertWithThisMod.Length > 0;
                    Position = InsertWithMod ? FileLinesLength : Position;
                    using (StreamWriter writer = new StreamWriter(MOModlistPath))
                    {
                        for (int LineNumber = 0; LineNumber < Position; LineNumber++)
                        {
                            if (InsertWithMod && FileLines[LineNumber].Length > 0 && string.Compare(FileLines[LineNumber].Remove(0, 1), InsertWithThisMod, true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (PlaceAfter)
                                {
                                    Position = LineNumber;
                                }
                                else
                                {
                                    writer.WriteLine(FileLines[LineNumber]);
                                    Position = LineNumber + 1;
                                }
                                break;

                            }
                            else
                            {
                                writer.WriteLine(FileLines[LineNumber]);
                            }
                        }

                        writer.WriteLine(Line);

                        if (Position < FileLinesLength)
                        {
                            for (int LineNumber = Position; LineNumber < FileLinesLength; LineNumber++)
                            {
                                writer.WriteLine(FileLines[LineNumber]);
                            }
                        }
                    }
                }
                else
                {
                    var prefix = "-";
                    if (IsEnabled)
                    {
                        prefix = "+";
                    }
                    FileLines = FileLines.Replace(prefix + Line.Remove(0, 1), Line);
                    File.WriteAllLines(MOModlistPath, FileLines);
                }
            }
        }
    }
}
