﻿namespace AIHelper.Games
{
    public class AISyoujyo : Game
    {
        public override string GetGameFolderName()
        {
            return GetTheGameFolderName(GetGameEXEName());
        }

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
    }
}
