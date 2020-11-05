using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Manage.Update.Targets
{
    class MO : TBase
    {
        public MO(updateInfo info) : base(info)
        {
        }

        string MODirPath;
        string MOExePath;

        /// <summary>
        /// Get MO update info
        /// </summary>
        /// <returns></returns>
        internal override Dictionary<string, string> GetUpdateInfos()
        {
            if (info.source.title.ToUpperInvariant().Contains("GITHUB"))
            {
                MODirPath = ManageSettings.GetMOdirPath();
                return new Dictionary<string, string>()
                {
                    { MODirPath, "ModOrganizer2,modorganizer,Mod.Organizer-"}
                };
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Update MO folder with new version
        /// </summary>
        /// <returns></returns>
        internal override bool UpdateFiles()
        {
            using (var installer = new Process())
            {
                installer.StartInfo.FileName = info.UpdateFilePath;
                installer.StartInfo.Arguments = "/dir=\"" + MODirPath + "\" /noicons /nocancel /norestart /silent";
                //installer.StartInfo.WorkingDirectory = Path.GetDirectoryName(info.UpdateFilePath);

                installer.Start();
                installer.WaitForExit();

                RestoreSomeFiles(info.BuckupDirPath, MODirPath);

                return installer.ExitCode==0;
            }
        }

        internal override void SetCurrentVersion()
        {
            info.UpdateFileEndsWith = ".exe";
            MOExePath = Path.Combine(MODirPath, "ModOrganizer.exe");
            if(File.Exists(MOExePath))
            {
                info.TargetCurrentVersion = FileVersionInfo.GetVersionInfo(MOExePath).ProductVersion;
            }
        }

        internal override string[] RestorePathsList()
        {
            return new[] 
            {
                @".\ModOrganizer.ini",
                @".\categories.dat",
                @".\plugins\modorganizer-basic_games",
                @".\explorer++\Explorer++RU.dll",
                @".\config.xml\Explorer++RU.dll",
            };
        }
    }
}
