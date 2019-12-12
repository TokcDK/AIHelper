using AI_Helper.Manage;
using System.IO;

namespace AI_Helper.Games
{
    public class HoneySelect : Game
    {
        public override void InitActions()
        {
            //var game = ManageSettings.GetListOfExistsGames()[ManageSettings.GetCurrentGameIndex()];
            string MO = Path.Combine(ManageSettings.GetCurrentGamePath(), "MO");
            string MOHS = Path.Combine(ManageSettings.GetCurrentGamePath(), "MOHS");
            if (Directory.Exists(MOHS) && !Directory.Exists(MO))
            {
                //Directory.CreateDirectory(MO);
                CopyFolder.Copy(Path.Combine(MOHS, "Profiles"), Path.Combine(MO, "Profiles"));
                CopyFolder.Copy(Path.Combine(MOHS, "Overwrite"), Path.Combine(MO, "Overwrite"));
                File.Copy(Path.Combine(MOHS, "categories.dat"), Path.Combine(MO, "categories.dat"));
                File.Copy(Path.Combine(MOHS, "ModOrganizer.ini"), Path.Combine(MO, "ModOrganizer.ini"));
            }
        }

        public override string GetGameFolderName()
        {
            return "HoneySelect";
        }

        public override string GetGameEXEName()
        {
            return "HoneySelect_64";
        }

        public override string GetGameEXENameX32()
        {
            return "HoneySelect_32";
        }

        public override string GetGameStudioEXEName()
        {
            return "StudioNEO_64";
        }

        public override string GetGameDisplayingName()
        {
            return T._("Honey Select");
        }
    }
}
