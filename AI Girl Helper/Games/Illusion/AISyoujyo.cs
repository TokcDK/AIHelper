namespace AIHelper.Games
{
    public class AISyoujyo : Game
    {
        private string GameFolderName = string.Empty;
        public override string GetGameFolderName()
        {
            return GameFolderName.Length > 0 ? GameFolderName : (GameFolderName = SearchGameFolder()).Length > 0 ? GameFolderName : GetGameEXEName();
        }

        public override string GetGameEXEName()
        {
            return "AI-Syoujyo";
        }

        public override string GetGameDisplayingName()
        {
            return T._("AI-Girl");
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEOV2";
        }
    }
}
