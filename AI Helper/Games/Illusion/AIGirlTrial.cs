namespace AIHelper.Games.Illusion
{
    public class AIGirlTrial : IllusionGameBase
    {
        public override string ManifestGame => "AI Girl";
        public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName(GetGameExeName());
        public override string GameExeName => "AI-SyoujyoTrial";

        public override string GameDisplayingName => T._("AI-Girl") + "Trial";

        public override string GameAbbreviation => "AI";

        public override BaseGamePyFileInfo BaseGamePyFile => new BaseGamePyFileInfo(nameof(Properties.Resources.game_aigirltrial), Properties.Resources.game_aigirltrial);
    }
}
