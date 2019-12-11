using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AI_Helper.Manage
{
    public class IniFile
    {
        //Материал: https://habr.com/ru/post/271483/

        readonly string Path; //Имя файла.

        [DllImport("kernel32", CharSet = CharSet.Unicode)] // Подключаем kernel32.dll и описываем его функцию WritePrivateProfilesString
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)] // Еще раз подключаем kernel32.dll, а теперь описываем функцию GetPrivateProfileString
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        // С помощью конструктора записываем путь до файла и его имя.
        public IniFile(string IniPath)
        {
            Path = new FileInfo(IniPath).FullName.ToString();
        }

        //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
        public string ReadINI(string Section, string Key)
        {
            var ini = ExIni.IniFile.FromFile(Path);
            var section = ini.GetSection(Section);
            if (section == null)
            {
                return string.Empty;
            }
            else
            {
                var key = section.GetKey(Key);

                if (key != null)//ExIni не умеет читать ключи с \\ в имени
                {
                    return key.Value;
                }
                else
                {
                    var RetVal = new StringBuilder(4096);
                    GetPrivateProfileString(Section, Key, "", RetVal, 4096, Path);//Это почему-то не может прочитать ключи из секций с определенными названиями
                    return RetVal.ToString();
                }
            }

        }
        //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
        public void WriteINI(string Section, string Key, string Value)
        { 
            if (!ManageStrings.IsStringAContainsStringB(Key, "\\"))
            {
                var ini = ExIni.IniFile.FromFile(Path);
                var section = ini.GetSection(Section);
                if (section != null)
                {
                    if (section.HasKey(Key))//ExIni не умеет читать ключи с \\ в имени
                    {
                        section.GetKey(Key).Value = Value;
                        ini.Save(Path);
                        return;
                    }
                    else
                    {
                        section.CreateKey(Key);
                        section.GetKey(Key).Value = Value;
                        ini.Save(Path);
                        return;
                    }
                }
            }
            WritePrivateProfileString(Section, Key, Value, Path);
        }

        //Удаляем ключ из выбранной секции.
        public void DeleteKey(string Key, string Section = null)
        {
            var ini = ExIni.IniFile.FromFile(Path);
            var section = ini.GetSection(Section);
            if (section != null)
            {
                if (section.HasKey(Key) && !ManageStrings.IsStringAContainsStringB(Key,"\\"))//ExIni не умеет читать ключи с \\ в имени
                {
                    section.DeleteKey(Key);
                    ini.Save(Path);
                }
                else
                {
                    WriteINI(Section, Key, null);
                }
            }
        }
        //Удаляем выбранную секцию
        public void DeleteSection(string Section = null)
        {
            var ini = ExIni.IniFile.FromFile(Path);
            if(Section!=null && ini.HasSection(Section))
            {
                ExIni.IniFile.FromFile(Path).DeleteSection(Section);
                ini.Save(Path);
            }
            //WriteINI(Section, null, null);
        }
        //Проверяем, есть ли такой ключ, в этой секции
        public bool KeyExists(string Key, string Section = null)
        {
            var iniSection = ExIni.IniFile.FromFile(Path).GetSection(Section);
            if (iniSection == null)
            {
                return false;
            }
            else
            {
                return iniSection.HasKey(Key);
            }
            //MessageBox.Show("key length="+ ReadINI(Section, Key).Length);
            //return ReadINI(Section, Key).Length > 0;
        }
    }
}
