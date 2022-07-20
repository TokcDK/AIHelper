namespace AIHelper.Games.Illusion
{
    public abstract class IllusionGameBase : GameBase
    {
        public override string RegistryPath => @"HKEY_CURRENT_USER\Software\illusion\" + GameExeName + @"\" + GameExeName;
        public override string RegistryInstallDirKey => "INSTALLDIR";

        public override string CharacterPresetsFolderSubPath => "UserData\\Chara";
    }
}
