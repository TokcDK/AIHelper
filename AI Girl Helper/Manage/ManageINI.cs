using System.IO;
using System.Linq;

namespace AIHelper.Manage
{
    internal static class ManageINI
    {
        public static string GetINIValueIfExist(string INIPath, string Key, string Section=null, string defaultValue = "")
        {
            if (!File.Exists(INIPath))
            {
                ManageMO.RedefineGameMOData();
            }

            Manage.INIFile INI = new Manage.INIFile(INIPath);
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
            (new Manage.INIFile(INIPath)).WriteINI(Section, Key, Value, DOSaveINI);
            return true;
            //return false;
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="MOiniPath"></param>
        /// <param name="line"></param>
        /// <param name="Position"></param>
        public static void InsertLineInFile(string MOiniPath, string Line, int Position = 1, string InsertWithThisMod = "", bool PlaceAfter = true)
        {
            if (MOiniPath.Length > 0 && File.Exists(MOiniPath) && Line.Length > 0)
            {
                string[] FileLines = File.ReadAllLines(MOiniPath);
                if (!FileLines.Contains(Line))
                {
                    int FileLinesLength = FileLines.Length;
                    bool InsertWithMod = InsertWithThisMod.Length > 0;
                    Position = InsertWithMod ? FileLinesLength : Position;
                    using (StreamWriter writer = new StreamWriter(MOiniPath))
                    {
                        for (int LineNumber = 0; LineNumber < Position; LineNumber++)
                        {
                            if (InsertWithMod && FileLines[LineNumber].Length > 0 && string.Compare(FileLines[LineNumber].Remove(0, 1), InsertWithThisMod, true) == 0)
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
            }
        }
    }
}
