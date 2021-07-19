﻿namespace AIHelper.UserData
{
    public class CharaFemale : IUserDataFolders
    {
        public string Foldername()
        {
            return "f";
        }

        public string TargetFolderSuffix()
        {
            return " Chars";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return "Chara";
        }

        public string TargetFolderName()
        {
            return "female";
        }
    }
}