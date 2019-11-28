using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.Utils
{
    class ManageINI
    {
        public static string GetINIValueIfExist(string INIPath, string Key, string Section, string defaultValue = "")
        {
            if (File.Exists(INIPath))
            {
                Utils.IniFile INI = new Utils.IniFile(INIPath);
                if (INI.KeyExists(Key, Section))
                {
                    return INI.ReadINI(Section, Key);
                }
            }
            return defaultValue;
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/8f713e50-0789-4bf6-865f-c87cdebd0b4f/insert-line-to-text-file-using-streamwriter-using-csharp?forum=csharpgeneral
        /// <summary>
        /// Inserts line in file in set position
        /// </summary>
        /// <param name="path"></param>
        /// <param name="line"></param>
        /// <param name="Position"></param>
        public static void InsertLineInFile(string path, string Line, int Position = 1, string InsertWithThisMod = "", bool PlaceAfter = true)
        {
            if (path.Length > 0 && File.Exists(path) && Line.Length > 0)
            {
                string[] FileLines = File.ReadAllLines(path);
                if (!FileLines.Contains(Line))
                {
                    int FileLinesLength = FileLines.Length;
                    bool InsertWithMod = InsertWithThisMod.Length > 0;
                    Position = InsertWithMod ? FileLinesLength : Position;
                    using (StreamWriter writer = new StreamWriter(path))
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
