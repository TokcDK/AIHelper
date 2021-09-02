namespace AIHelper.Games
{
    public class AiSyoujyo : Game
    {
        public override string GetGameFolderName()
        {
            return base.GetGameFolderName();

            //return GetTheGameFolderName(GetGameExeName());
        }
        public override bool IsHaveSideloaderMods { get; set; } = true;

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

        public override string GetGamePrefix()
        {
            return "AI";
        }

        public override BaseGamePyFileInfo GetBaseGamePyFile()
        {
            return new BaseGamePyFileInfo(nameof(Properties.Resources.game_aigirl), Properties.Resources.game_aigirl);
        }
    }
}
