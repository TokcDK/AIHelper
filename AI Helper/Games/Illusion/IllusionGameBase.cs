namespace AIHelper.Games.Illusion
{
    abstract class IllusionGameBase : GameBase
    {
        public override string RegistryPath => @"HKEY_CURRENT_USER\Software\illusion\" + GetGameExeName() + @"\" + GetGameExeName();
        public override string RegistryInstallDirKey => "INSTALLDIR";
    }
}
