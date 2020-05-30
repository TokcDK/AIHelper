namespace AIHelper.UserData
{
    public class Overlays : IUserDataFolders
    {
        public string Foldername()
        {
            return "o";
        }

        public string TargetFolderSuffix()
        {
            return " Overlays";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return string.Empty;
        }

        public string TargetFolderName()
        {
            return "Overlays";
        }
    }
}
