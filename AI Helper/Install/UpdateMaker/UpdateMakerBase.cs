using System.Collections.Generic;

namespace AIHelper.Install.UpdateMaker
{
    /// <summary>
    /// Base class of the update makers which are describing a content type of the game dir
    /// </summary>
    abstract class UpdateMakerBase
    {
        /// <summary>
        /// Paths which must be excluded from the update
        /// </summary>
        public HashSet<string> Blacklist { get; set; } = new HashSet<string>();

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
        /// True when at least one file of the content type was copied to the update dir
        /// </summary>
        public bool IsAnyFileCopied { get; set; }
    }
}
