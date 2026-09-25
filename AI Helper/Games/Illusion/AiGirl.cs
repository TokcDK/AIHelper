namespace AIHelper.Games.Illusion
{
    public class AiGirl : IllusionGameBase
    {
        public override string ZipmodManifestGameName => "AI Girl";

        public override string GameExeName => "AI-Syoujyo";

        public override string GameDisplayingName => T._("AI-Girl");

        public override string GameStudioExeName => "StudioNEOV2";

        public override string GameAbbreviation => "AI";

        public override string BasicGamePluginName => "game_aigirl";
    }
}
