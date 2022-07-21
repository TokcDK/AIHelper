namespace AIHelper.Games.Illusion
{
    public class AIGirlTrial : IllusionGameBase
    {
        public override string ZipmodManifestGameName => "AI Girl";
        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName(GetGameExeName());
        public override string GameExeName => "AI-SyoujyoTrial";

        public override string GameDisplayingName => T._("AI-Girl") + "Trial";

        public override string GameAbbreviation => "AI";

        public override string BasicGamePluginName => "game_aigirltrial";
    }
}
