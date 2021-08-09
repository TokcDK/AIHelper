using System.IO;

namespace AIHelper.Install.Types.Directories
{
    abstract class DirectoriesInstallerBase : ModInstallerBase
    {
        public override string[] Masks => new[] { "*" };

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

            foreach (var mask in Masks)
            {
                Mask = mask;

                var list = directoryInfo.GetDirectories(mask);
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].Exists)
                    {
                        Get(list[i]);
                    }
                }
            }
        }

        protected abstract void Get(DirectoryInfo directoryInfo);
    }
}
