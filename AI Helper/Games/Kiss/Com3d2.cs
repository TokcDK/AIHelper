using System.IO;
using System.Windows.Forms;

namespace AIHelper.Games.Kiss
{
    class Com3d2 : KissGameBase
    {
        public override void InitActions()
        {
            base.InitActions();
        }

        //english is 'HKEY_CURRENT_USER\Software\KISS\CUSTOM ORDER MAID3D 2'
        public override string RegistryPath => @"HKEY_CURRENT_USER\Software\KISS\カスタムオーダーメイド3D2";
        public override string RegistryInstallDirKey => "InstallPath";

        public override string ZipmodManifestGameName => "com3d2";

        //public override string GameDirName => base.GameDirName;
        //return GetTheGameFolderName("Koikatsu");
        public override string GameExeName => "COM3D2x64";
        public override string GameExeNameX32 => "COM3D2";

        public override string GameDisplayingName => T._("Custom Order Maid 3D2");

        public override string GameStudioExeName => "CharaStudio";

        public override string GameAbbreviation => "COM3D2";

        public override string BasicGamePluginName => "game_com3d2";
    }
}
