﻿using System;
using System.Collections.Generic;
using System.IO;

namespace AIHelper.Manage.Update.Targets
{
    class MOBaseGames : TBase
    {
        public MOBaseGames(updateInfo info) : base(info)
        {
        }

        internal override Dictionary<string, string> GetUpdateInfos()
        {
            if (info.source.title.ToUpperInvariant().Contains("GITHUB"))
            {
                info.GetVersionFromLink = true;
                return new Dictionary<string, string>()
                {
                    { Path.Combine(ManageSettings.GetMOdirPath(), "plugins", "modorganizer-basic_games-master"), info.SourceLink="https://github.com/ModOrganizer2/modorganizer-basic_games/archive/master.zip" }
                };
            }
            else
            {
                return null;
            }
        }

        internal override void SetCurrentVersion()
        {
            throw new NotImplementedException();
        }

        internal override bool UpdateFiles()
        {
            throw new NotImplementedException();
        }
    }
}
