namespace AI_Helper.Games
{
    public class AISyoujyo : Game
    {
        public override string GetGameFolderName()
        {
            return GetGameEXEName();
        }

        public override string GetGameEXEName()
        {
            return "AI-Syoujyo";
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEOV2";
        }
    }
}
