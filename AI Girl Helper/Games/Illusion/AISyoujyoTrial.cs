namespace AIHelper.Games
{
    public class AISyoujyoTrial : Game
    {
        public override string GetGameFolderName()
        {
            return GetTheGameFolderName(GetGameEXEName());
        }

        public override string GetGameEXEName()
        {
            return "AI-SyoujyoTrial";
        }

        public override string GetGameDisplayingName()
        {
            return T._("AI-Girl") + "Trial";
        }

        public override string GetGamePrefix()
        {
            return "AI";
        }

        internal override System.Collections.Generic.Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            return new System.Collections.Generic.Dictionary<string, byte[]>
                {
                    { nameof(Properties.Resources.game_aigirltrial), Properties.Resources.game_aigirltrial }
                };
        }
    }
}
