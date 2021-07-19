//using System;
//using System.Collections.Generic;
//using System.IO;

//namespace AIHelper.Manage.Update.Targets
//{
//    /// <summary>
//    /// basic games page https://github.com/ModOrganizer2/modorganizer-basic_games is changing with sources and releases not updating
//    /// and also it has different versions for different MO and because currently better to update it manually
//    /// </summary>
//    /// <returns></returns>
//    class MOBaseGames : TBase
//    {
//        //https://github.com/ModOrganizer2/modorganizer-basic_games
//        public MOBaseGames(updateInfo info) : base(info)
//        {
//        }

//        internal override Dictionary<string, string> GetUpdateInfos()
//        {
//            if (info.source.title.ToUpperInvariant().Contains("GITHUB"))
//            {
//                info.GetVersionFromLink = true;
//                //string moversion = ManageMO.GetMOVersion();
//                return new Dictionary<string, string>()
//                    {
//                        { Path.Combine(ManageSettings.GetMOdirPath(), "plugins", "modorganizer-basic_games"), info.SourceLink="https://github.com/ModOrganizer2/modorganizer-basic_games/archive/master.zip" }
//                    };
//            }
//            else
//            {
//                return null;
//            }
//        }

//        internal override void SetCurrentVersion()
//        {
//            var file = Path.Combine(ManageSettings.GetMOdirPath(), "plugins", "modorganizer-basic_games", "ver");

//            info.TargetCurrentVersion = File.ReadAllLines(file)[0];
//        }

//        internal override bool UpdateFiles()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
