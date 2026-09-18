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
            return ProcessEntries(directoryInfo, mask => directoryInfo.GetFiles(mask), Get, "file installer");
        }

        protected abstract bool Get(FileInfo fileInfo);
    }
}
