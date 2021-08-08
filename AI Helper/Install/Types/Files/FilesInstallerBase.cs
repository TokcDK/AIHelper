using System.IO;

namespace AIHelper.Install.Types.Files
{
    abstract class FilesInstallerBase : ModInstallerBase
    {
        public override int Order => base.Order / 2;

        public override string Mask => "*.*";

        public override void Install()
        {
            GetAll(new DirectoryInfo(Manage.ManageSettings.GetInstall2MoDirPath()));
        }

        public override void InstallFrom(string path)
        {
            GetAll(new DirectoryInfo(path));
        }

        protected void GetAll(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists)
            {
                return;
            }

            var list = directoryInfo.GetFiles(Mask);
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Exists)
                {
                    Get(list[i]);
                }
            }
        }

        protected abstract void Get(FileInfo fileInfo);
    }
}
