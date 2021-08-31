namespace AIHelper.Games
{
    public class AiSyoujyoTrial : Game
    {
        public override string ManifestGame => "AI Girl";
        public override string GetGameFolderName()
        {
            return base.GetGameFolderName();

            //return GetTheGameFolderName(GetGameExeName());
        }

        public override string GetGameExeName()
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

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_aigirltrial), Properties.Resources.game_aigirltrial);
        }
    }
}
