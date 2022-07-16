namespace AIHelper.UserData
{
    public class StudioScene : IUserDataFolders
    {
        public string Foldername => "s";

        public string TargetFolderSuffix => " Scenes";

        public string Extension => ".png";

        public string TypeFolder => "studio";

        public string TargetFolderName => "scene";
    }
}
