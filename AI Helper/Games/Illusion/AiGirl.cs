namespace AIHelper.Games.Illusion
{
    public class AiGirl : IllusionGameBase
    {
        public override string GetGameDirName()
        {
            return base.GetGameDirName();

            //return GetTheGameFolderName(GetGameExeName());
        }
        public override bool IsHaveSideloaderMods => true;

        public override string ManifestGame => "AI Girl";

        public override string GetGameExeName()
        {
            return "AI-Syoujyo";
        }

        public override string GetGameDisplayingName()
        {
            return T._("AI-Girl");
        }

        public override string GetGameStudioExeName()
        {
            return "StudioNEOV2";
        }

        public override string GetGameAbbreviation()
        {
            return "AI";
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_aigirl), Properties.Resources.game_aigirl);
        }
    }
}
