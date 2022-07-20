namespace AIHelper.Games.Illusion
{
    public class AiGirl : IllusionGameBase
    {
        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName(GetGameExeName());                                                                public override bool IsHaveSideloaderMods => true;

        public override string ManifestGame => "AI Girl";

        public override string GameExeName => "AI-Syoujyo";

        public override string GameDisplayingName => T._("AI-Girl");

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameAbbreviation => "AI";

        public override BaseGamePyFileInfo BaseGamePyFile => new BaseGamePyFileInfo(nameof(Properties.Resources.game_aigirl), Properties.Resources.game_aigirl);
    }
}
