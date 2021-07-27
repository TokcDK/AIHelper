namespace AIHelper.Manage
{
    static class ManageMoModsExtensions
    {
        internal static bool IsInOverwriteFolder(this string filePath)
        {
            return filePath.ToUpperInvariant().Contains(ManageSettings.GetOverwriteFolder().ToUpperInvariant());
        }
    }
}
