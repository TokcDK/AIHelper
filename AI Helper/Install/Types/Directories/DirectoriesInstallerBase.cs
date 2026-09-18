using System.IO;

namespace AIHelper.Install.Types.Directories
{
    abstract class DirectoriesInstallerBase : ModInstallerBase
    {
        public override string[] Masks => new[] { "*" };

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
            return ProcessEntries(directoryInfo, mask => directoryInfo.GetDirectories(mask), Get, "dir installer");
        }

        protected abstract bool Get(DirectoryInfo directoryInfo);
    }
}
