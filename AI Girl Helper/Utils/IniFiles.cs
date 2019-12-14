using IniParser;
using IniParser.Model;
using System.IO;

namespace AIHelper.Manage
{
    public class IniFile
    {
        //Материал: https://habr.com/ru/post/271483/

        private readonly string Path; //Имя файла.
        private readonly FileIniDataParser INIParser;
        private readonly IniData INIData;
        bool ActionWasExecuted = false;

        //[DllImport("kernel32", CharSet = CharSet.Unicode)] // Подключаем kernel32.dll и описываем его функцию WritePrivateProfilesString
        //static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        //[DllImport("kernel32", CharSet = CharSet.Unicode)] // Еще раз подключаем kernel32.dll, а теперь описываем функцию GetPrivateProfileString
        //static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        //public IniData GetINIData()
        //{
        //    return INIData;
        //}

        // С помощью конструктора записываем путь до файла и его имя.
        public IniFile(string IniPath)
        {
            Path = new FileInfo(IniPath).FullName.ToString();
            INIParser = new FileIniDataParser();
            if (!File.Exists(Path))
            {
                File.WriteAllText(Path, string.Empty);
            }
            INIData = INIParser.ReadFile(Path);
        }

        //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
        public string ReadINI(string Section, string Key)
        {
            if (string.IsNullOrEmpty(Section))
            {
                return INIData.Global[Key];
            }
            else
            {
                if (!INIData.Sections.ContainsSection(Section))
                {
                    return string.Empty;
                }
                return INIData[Section][Key];
            }

            //var ini = ExIni.IniFile.FromFile(Path);
            //var section = ini.GetSection(Section);
            //if (section == null)
            //{
            //    return string.Empty;
            //}
            //else
            //{
            //    var key = section.GetKey(Key);

            //    if (key != null)//ExIni не умеет читать ключи с \\ в имени
            //    {
            //        return key.Value;
            //    }
            //    else
            //    {
            //        var RetVal = new StringBuilder(4096);
            //        GetPrivateProfileString(Section, Key, "", RetVal, 4096, Path);//Это почему-то не может прочитать ключи из секций с определенными названиями
            //        return RetVal.ToString();
            //    }
            //}

        }
        //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
        public void WriteINI(string Section, string Key, string Value, bool DoSaveINI = true)
        {
            if (string.IsNullOrEmpty(Section))
            {
                INIData.Global[Key] = Value;
                ActionWasExecuted = true;
            }
            else
            {
                if (!INIData.Sections.ContainsSection(Section))
                {
                    INIData.Sections.AddSection(Section);
                }
                INIData[Section][Key] = Value;
                ActionWasExecuted = true;
            }

            if (DoSaveINI && ActionWasExecuted)
            {
                INIParser.WriteFile(Path, INIData);
            }
            //if (!ManageStrings.IsStringAContainsStringB(Key, "\\"))
            //{
            //    var ini = ExIni.IniFile.FromFile(Path);
            //    var section = ini.GetSection(Section);
            //    if (section != null)
            //    {
            //        if (section.HasKey(Key))//ExIni не умеет читать ключи с \\ в имени
            //        {
            //            section.GetKey(Key).Value = Value;
            //            ini.Save(Path);
            //            return;
            //        }
            //        else
            //        {
            //            section.CreateKey(Key);
            //            section.GetKey(Key).Value = Value;
            //            ini.Save(Path);
            //            return;
            //        }
            //    }
            //}
            //WritePrivateProfileString(Section, Key, Value, Path);
        }

        //Удаляем ключ из выбранной секции.
        public void DeleteKey(string Key, string Section = null, bool DoSaveINI = true)
        {
            if (string.IsNullOrEmpty(Section))
            {
                INIData.Global.RemoveKey(Key);
                ActionWasExecuted = true;
            }
            else
            {
                if (INIData.Sections.ContainsSection(Section))
                {
                    INIData[Section].RemoveKey(Key);
                    ActionWasExecuted = true;
                }
            }
            if (DoSaveINI && ActionWasExecuted)
            {
                INIParser.WriteFile(Path, INIData);
            }
            //var ini = ExIni.IniFile.FromFile(Path);
            //var section = ini.GetSection(Section);
            //if (section != null)
            //{
            //    if (section.HasKey(Key) && !ManageStrings.IsStringAContainsStringB(Key,"\\"))//ExIni не умеет читать ключи с \\ в имени
            //    {
            //        section.DeleteKey(Key);
            //        ini.Save(Path);
            //    }
            //    else
            //    {
            //        WriteINI(Section, Key, null);
            //    }
            //}
        }
        //Удаляем выбранную секцию
        public void DeleteSection(string Section/* = null*/, bool DoSaveINI = true)
        {
            if (INIData.Sections.ContainsSection(Section))
            {
                INIData.Sections.RemoveSection(Section);
                ActionWasExecuted = true;
            }
            if (DoSaveINI && ActionWasExecuted)
            {
                INIParser.WriteFile(Path, INIData);
            }
            //var ini = ExIni.IniFile.FromFile(Path);
            //if(Section!=null && ini.HasSection(Section))
            //{
            //    ExIni.IniFile.FromFile(Path).DeleteSection(Section);
            //    ini.Save(Path);
            //}
            //WriteINI(Section, null, null);
        }
        //Проверяем, есть ли такой ключ, в этой секции
        public bool KeyExists(string Key, string Section = null)
        {
            if (string.IsNullOrEmpty(Section))
            {
                return INIData.Global.ContainsKey(Key);
            }
            else
            {
                if (!INIData.Sections.ContainsSection(Section))
                {
                    return false;
                }
                return INIData[Section].ContainsKey(Key);
            }
            //var iniSection = ExIni.IniFile.FromFile(Path).GetSection(Section);
            //if (iniSection == null)
            //{
            //    return false;
            //}
            //else
            //{
            //    return iniSection.HasKey(Key);
            //}
            //MessageBox.Show("key length="+ ReadINI(Section, Key).Length);
            //return ReadINI(Section, Key).Length > 0;
        }
    }
}
