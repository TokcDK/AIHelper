namespace AIHelper.Games
{
    public class AISyoujyoTrial : Game
    {
        private string GameFolderName = string.Empty;
        public override string GetGameFolderName()
        {
            return GameFolderName.Length > 0 ? GameFolderName : (GameFolderName = SearchGameFolder()).Length > 0 ? GameFolderName : GetGameEXEName();
        }

        public override string GetGameEXEName()
        {
            return "AI-SyoujyoTrial";
        }

        public override string GetGameDisplayingName()
        {
            return T._("AI-Girl")+"Trial";
        }
    }
}
