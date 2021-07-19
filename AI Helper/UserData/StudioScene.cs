namespace AIHelper.UserData
{
    public class StudioScene : IUserDataFolders
    {
        public string Foldername()
        {
            return "s";
        }

        public string TargetFolderSuffix()
        {
            return " Scenes";
        }

        public string Extension()
        {
            return ".png";
        }

        public string TypeFolder()
        {
            return "studio";
        }

        public string TargetFolderName()
        {
            return "scene";
        }
    }
}
