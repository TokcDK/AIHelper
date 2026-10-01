using AIHelper.Manage;
using System.Globalization;
using System.IO;

namespace AIHelper.Install.Types.Files
{
    /// <summary>
    /// Installs cs scripts to the scripts subdir of their mod dir
    /// </summary>
    class CsScriptsInstaller : FilesInstallerBase
    {
        public override string[] Masks => new[] { "*.cs" };

        public override string ContentDescription => T._("ScriptLoader scripts (*.cs)");

        protected override bool Get(FileInfo csFile)
        {
            string name = Path.GetFileNameWithoutExtension(csFile.Name);
            string author = string.Empty;
            string modName = "[script]" + name;
            string modDir = Path.Combine(ManageSettings.CurrentGameModsDirPath, modName);

            string description = ReadDescription(csFile);

            string scriptsDir = Path.Combine(modDir, "scripts");
            if (!Directory.Exists(scriptsDir))
            {
                Directory.CreateDirectory(scriptsDir);
            }

            string fileTargetPath = Path.Combine(scriptsDir, name + ".cs");

            // time of the installing file must be read before the file is moved or deleted
            string fileLastModificationTime = File.GetLastWriteTime(csFile.FullName).ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);

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

            //запись meta.ini
            ManageModOrganizer.WriteMetaIni(
                modDir
                ,
                isUpdate ? string.Empty : "ScriptLoader scripts"
                ,
                "0." + fileLastModificationTime
                ,
                isUpdate ? string.Empty : "Requires: " + "ScriptLoader"
                ,
                isUpdate ? string.Empty : "<br>" + "Author" + ": " + author + "<br><br>" + (description.Length > 0 ? description : name)
                );

            ManageModOrganizer.ActivateDeactivateInsertMod(modName, false, "ScriptLoader scripts_separator");

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

        /// <summary>
        /// Reads the description of the script from its head comment
        /// </summary>
        /// <param name="csFile">Installing cs file</param>
        /// <returns>Description of the script</returns>
        private static string ReadDescription(FileInfo csFile)
        {
            string description = string.Empty;

            using (var sReader = new StreamReader(csFile.FullName))
            {
                bool readDescriptionMode = false;
                while (!sReader.EndOfStream)
                {
                    string line = sReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

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
                }
            }

            return description;
        }
    }
}
