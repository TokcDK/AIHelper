namespace AIHelper.UserData
{
    class CardframeBack : IUserDataFolders
    {
        public string Foldername()
        {
            return "cf";
        }

        public string TargetFolderSuffix()
        {
            return " Cardframes";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return "cardframe";
        }

        public string TargetFolderName()
        {
            return "Back";
        }
    }
}
