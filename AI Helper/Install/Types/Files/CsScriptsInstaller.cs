using AIHelper.Manage;
using System.Globalization;
using System.IO;

namespace AIHelper.Install.Types.Files
{
    class CsScriptsInstaller : FilesInstallerBase
    {
        public override string[] Masks => new[] { "*.cs" };

        protected override bool Get(FileInfo csFile)
        {
            string name = Path.GetFileNameWithoutExtension(csFile.Name);
            string author = string.Empty;
            string description = string.Empty;
            string modname = "[script]" + name;
            string moddir = Path.Combine(ManageSettings.GetCurrentGameModsDirPath(), modname);

            using (StreamReader sReader = new StreamReader(csFile.FullName))
            {
                string line;
                bool readDescriptionMode = false;
                int i = 0;
                while (!sReader.EndOfStream || (!readDescriptionMode && i == 10))
                {
                    line = sReader.ReadLine();

                    if (!readDescriptionMode /*&& Line.Length > 0 уже есть эта проверка в StringEx.IsStringAContainsStringB*/ && ManageStrings.IsStringAContainsStringB(line, "/*"))
                    {
                        readDescriptionMode = true;
                        line = line.Replace("/*", string.Empty);
                        if (line.Length > 0)
                        {
                            description += line + "<br>";
                        }
                    }
                    else
                    {
                        if (ManageStrings.IsStringAContainsStringB(line, "*/"))
                        {
                            readDescriptionMode = false;
                            line = line.Replace("*/", string.Empty);
                        }

                        description += line + "<br>";

                        if (!readDescriptionMode)
                        {
                            break;
                        }
                    }

                    i++;
                }
            }
            string scriptsdir = Path.Combine(moddir, "scripts");
            if (!Directory.Exists(scriptsdir))
            {
                Directory.CreateDirectory(scriptsdir);
            }

            string fileTargetPath = Path.Combine(scriptsdir, name + ".cs");
            bool isUpdate = false;
            //резервная копия, если файл существовал
            if (File.Exists(fileTargetPath))
            {
                isUpdate = true;
                if (File.GetLastWriteTime(csFile.FullName) > File.GetLastWriteTime(fileTargetPath))
                {
                    File.Delete(fileTargetPath);
                    csFile.MoveTo(fileTargetPath);
                }
                else
                {
                    csFile.Delete();
                }
            }
            else
            {
                csFile.MoveTo(fileTargetPath);
            }

            string fileLastModificationTime = File.GetLastWriteTime(csFile.FullName).ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
            //запись meta.ini
            ManageModOrganizer.WriteMetaIni(
                moddir
                ,
                isUpdate ? string.Empty : "ScriptLoader scripts"
                ,
                "0." + fileLastModificationTime
                ,
                isUpdate ? string.Empty : "Requires: " + "ScriptLoader"
                ,
                isUpdate ? string.Empty : "<br>" + "Author" + ": " + author + "<br><br>" + (description.Length > 0 ? description : name)
                );

            ManageModOrganizer.ActivateDeactivateInsertMod(modname, false, "ScriptLoader scripts_separator");

            return true;

            //string[] extrafiles = Directory.GetFiles(whereFromInstallDir, name + "*.*");
            //if (extrafiles.Length > 0)
            //{
            //    foreach (var extrafile in extrafiles)
            //    {
            //        string targetFile = extrafile.Replace(whereFromInstallDir, moddir);

            //        if (File.Exists(targetFile))
            //        {
            //            if (File.GetLastWriteTime(extrafile) > File.GetLastWriteTime(targetFile))
            //            {
            //                File.Delete(targetFile);
            //                File.Move(extrafile, targetFile);
            //            }
            //            else
            //            {
            //                File.Delete(extrafile);
            //            }
            //        }
            //        else
            //        {
            //            File.Move(extrafile, targetFile);
            //        }
            //    }
            //}
        }
    }
}
