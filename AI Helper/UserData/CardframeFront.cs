namespace AIHelper.UserData
{
    class CardframeFront : IUserDataFolders
    {
        public string Foldername => "cf";

        public string TargetFolderSuffix => " Cardframes";

        public string Extension => ".png";

        public string TypeFolder => "cardframe";

        public string TargetFolderName => "Front";
    }
}
