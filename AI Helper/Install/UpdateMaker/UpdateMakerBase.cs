namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Describes one content type of the game dir which can be packed into an update
    /// </summary>
    /// <remarks>
    /// A maker holds no state: it only names the content type and the update maker ini keys
    /// which belong to it, so the same instance can be asked for the same answers at any time.
    /// </remarks>
    abstract class UpdateMakerBase
    {
        /// <summary>
        /// Directory name of the content type in the game dir
        /// </summary>
        public abstract string DirName { get; }

        /// <summary>
        /// Key name with the list of the content type dirs
        /// </summary>
        public virtual string DirsKey => DirName + "Dirs";

        /// <summary>
        /// Key name with the list of the content type files
        /// </summary>
        public virtual string FilesKey => DirName + "Files";

        /// <summary>
        /// Key name of the game update flag which tells the installer to update this content type
        /// </summary>
        public virtual string UpdateKey => "Update" + DirName;
    }
}
