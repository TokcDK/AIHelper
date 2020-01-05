namespace AIHelper.Games
{
    public class HoneySelect : Game
    {
        public override void InitActions()
        {
            CopyMOfiles("MOHS");
        }

        private string GameFolderName = string.Empty;

        public override string GetGameFolderName()
        {
            return GameFolderName.Length > 0 ? GameFolderName : GameFolderName = SearchGameFolder();
        }

        public override string GetGameEXEName()
        {
            return "HoneySelect_64";
        }

        public override string GetGameEXENameX32()
        {
            return "HoneySelect_32";
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEO_64";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select");
        }
    }
}
