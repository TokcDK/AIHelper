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
        public abstract bool Install();

        /// <summary>
        /// install from specific folder
        /// </summary>
        /// <param name="path"></param>
        public abstract bool InstallFrom(string path);

        /// <summary>
        /// mask of installer's objects
        /// </summary>
        public abstract string[] Masks { get; }

        /// <summary>
        /// currently selected mask
        /// </summary>
        protected string Mask;
    }
}
