namespace AIHelper.UserData
{
    public class Housing : IUserDataFolders
    {
        public string Foldername => "h";

        public string TargetFolderSuffix => " Housing";

        public string Extension => ".png";

        public string TypeFolder => string.Empty;

        public string TargetFolderName => "housing";
    }
}
