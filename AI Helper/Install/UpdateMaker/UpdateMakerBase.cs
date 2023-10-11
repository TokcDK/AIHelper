using System.Collections.Generic;

namespace AIHelper.Install.UpdateMaker
{
    abstract class UpdateMakerBase
    {
        public HashSet<string> blacklist;

        public abstract string DirName { get; }
        public virtual string DirsKey => DirName + "Dirs";
        public virtual string FilesKey => DirName + "Files";

        public bool IsAnyFileCopied;
    }
}
