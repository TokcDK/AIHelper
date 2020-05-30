﻿namespace AIHelper.Games
{
    public class AISyoujyoTrial : Game
    {
        public override string GetGameFolderName()
        {
            return GetTheGameFolderName(GetGameEXEName());
        }

        public override string GetGameEXEName()
        {
            return "AI-SyoujyoTrial";
        }

        public override string GetGameDisplayingName()
        {
            return T._("AI-Girl") + "Trial";
        }
    }
}
