﻿namespace AIHelper.Games
{
    public class AISyoujyo : Game
    {
        public override string GetGameFolderName()
        {
            return GetTheGameFolderName(GetGameEXEName());
        }
        public override bool isHaveSideloaderMods { get; set; } = true;

        public override string GetGameEXEName()
        {
            return "AI-Syoujyo";
        }

        public override string GetGameDisplayingName()
        {
            return T._("AI-Girl");
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEOV2";
        }

        public override string GetGamePrefix()
        {
            return "AI";
        }

        public override System.Collections.Generic.Dictionary<string, byte[]> GetBaseGamePyFile()
        {
            return new System.Collections.Generic.Dictionary<string, byte[]>
                {
                    { nameof(Properties.Resources.game_aigirl), Properties.Resources.game_aigirl }
                };
        }
    }
}