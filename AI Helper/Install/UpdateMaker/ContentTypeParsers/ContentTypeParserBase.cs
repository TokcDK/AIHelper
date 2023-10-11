using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Install.UpdateMaker
{
    partial class UpdateMaker
    {
        abstract class ContentTypeParserBase
        {
            protected INIFileMan.INIFile Ini;
            internal UpdateMakerBase UpdateMaker;

            protected ContentTypeParserBase(INIFileMan.INIFile ini)
            {
                Ini = ini;
            }

            public bool IsNeedToCopy { get => Ini.KeyExists("", PathsKeyName); }
            protected abstract string PathsKeyName { get; }
            protected abstract string BlacklistKeyName { get; }
            public abstract string RemoveKeyName { get; }
            public HashSet<string> RemoveList = new HashSet<string>();
            public HashSet<string> IncludedList = new HashSet<string>();
            protected bool UseBlacklist = false;

            public void Copy(string gameDirPath, string updateDirPath)
            {
                var includedPathsValue = Ini.GetKey("", PathsKeyName);
                if (includedPathsValue != null)
                {
                    IncludedList = includedPathsValue.Split(',').Distinct().ToHashSet();
                }

                var pathsValue = Ini.GetKey("", BlacklistKeyName);
                if (pathsValue != null)
                {
                    RemoveList = pathsValue.Split(',').ToHashSet();
                    UpdateMaker.blacklist = RemoveList;
                    if (RemoveList.Count > 0)
                    {
                        UseBlacklist = true;

                        foreach (var subpath in EnumerateSubpathsToRemove(RemoveList, gameDirPath, updateDirPath))
                        {
                            var blacklistedSubPath = gameDirPath + Path.DirectorySeparatorChar + subpath;
                            if (Directory.Exists(blacklistedSubPath) && !RemoveList.Contains(subpath))
                            {
                                if(!RemoveList.Contains(subpath)) RemoveList.Add(subpath);
                            }
                        }
                    }
                }

                if (IncludedList.Count > 0) Copy(IncludedList, gameDirPath, updateDirPath);
            }
            protected abstract void Copy(HashSet<string> paths, string gameDirPath, string updateDirPath);

            protected abstract IEnumerable<string> EnumerateSubpathsToRemove(HashSet<string> paths, string gameDirPath,string updateDirPath);
        }
    }
}
