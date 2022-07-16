namespace AIHelper.UserData
{
    public class Coordinate : IUserDataFolders
    {
        public string Foldername => "m";

        public string TargetFolderSuffix => " Coordinate";

        public string Extension => ".png";

        public string TypeFolder => string.Empty;

        public string TargetFolderName => "coordinate";
    }
}
