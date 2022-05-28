namespace AIHelper.Games.Illusion
{
    public class AIGirlTrial : IllusionGameBase
    {
        public override string ManifestGame => "AI Girl";
        public override string GetGameDirName()
        {
            return base.GetGameDirName();

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

        public override string GetGameAbbreviation()
        {
            return "AI";
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_aigirltrial), Properties.Resources.game_aigirltrial);
        }
    }
}
