using AIHelper.Forms.Other;
using System;
using System.IO;

namespace AIHelper.Install.Types.Files
{
    abstract class FilesInstallerBase : ModInstallerBase
    {
        public override int Order => base.Order / 2;

        public override string[] Masks => new[] { "*.*" };

        public override bool Install()
        {
            return GetAll(new DirectoryInfo(Manage.ManageSettings.Install2MoDirPath));
        }

        public override bool InstallFrom(string path)
        {
            return GetAll(new DirectoryInfo(path));
        }

        protected bool GetAll(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists)
            {
                return false;
            }

            bool ret = false;

            ProgressForm progress = new ProgressForm();

            foreach (var mask in Masks)
            {
                Mask = mask;

                var list = directoryInfo.GetFiles(mask);

                progress.SetMax(list.Length);

                for (int i = 0; i < list.Length; i++)
                {
                    progress.SetProgress(i);

                    if (list[i].Exists)
                    {
                        progress.SetInfo(list[i].Name);

                        try
                        {
                            if (Get(list[i]))
                            {
                                ret = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error("An error occured while parse by file installer. error\r\n"+ex);
                        }
                    }
                }
            }

            progress.Dispose();

            return ret;
        }

        protected abstract bool Get(FileInfo fileInfo);
    }
}
