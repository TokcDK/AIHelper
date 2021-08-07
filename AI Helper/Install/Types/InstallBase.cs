using AIHelper.Manage;
using System.IO;

namespace AIHelper.Install.Types
{
    public abstract class InstallerByTypeBase
    {
        //public DirectoryInfo SourceDir { get; set; }

        public abstract bool Install();

        readonly DirectoryInfo ToMO = new DirectoryInfo(ManageSettings.GetInstall2MoDirPath());

        protected virtual string Mask { get => "*"; }

        protected void GetFiles()
        {
            var list = ToMO.GetFiles(Mask);
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Exists)
                {
                    GetFile(list[i]);
                }
            }
        }

        protected virtual void GetFile(FileInfo fileInfo)
        {
        }

        protected void GetDirs()
        {
            var list = ToMO.GetDirectories(Mask);
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Exists)
                {
                    GetDir(list[i]);
                }
            }
        }

        protected virtual void GetDir(DirectoryInfo directoryInfo)
        {
        }
    }
}
