namespace AIHelper.Games.Illusion
{
    public abstract class IllusionGameBase : GameBase
    {
        public override string RegistryPath => @"HKEY_CURRENT_USER\Software\illusion\" + GetGameExeName() + @"\" + GetGameExeName();
        public override string RegistryInstallDirKey => "INSTALLDIR";

        public override string GetCharacterPresetsFolderSubPath()
        {
            return "UserData\\Chara";
        }
    }
}
