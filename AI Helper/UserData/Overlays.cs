namespace AIHelper.UserData
{
    public class Overlays : IUserDataFolders
    {
        public string Foldername => "o";

        public string TargetFolderSuffix => " Overlays";

        public string Extension => ".png";

        public string TypeFolder => string.Empty;

        public string TargetFolderName => "Overlays";
    }
}
