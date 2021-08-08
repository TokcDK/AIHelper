namespace AIHelper.Install.Types
{
    abstract class ModInstallerBase
    {
        /// <summary>
        /// order of installer
        /// </summary>
        public virtual int Order => 1000;

        /// <summary>
        /// install from default 2MO folder
        /// </summary>
        public abstract void Install();

        /// <summary>
        /// install from specific folder
        /// </summary>
        /// <param name="path"></param>
        public abstract void InstallFrom(string path);

        /// <summary>
        /// mask of installer's objects
        /// </summary>
        public abstract string Mask { get; }
    }
}
