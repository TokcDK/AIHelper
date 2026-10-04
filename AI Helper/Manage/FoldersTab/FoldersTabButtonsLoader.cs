using AIHelper.Manage.ui;

namespace AIHelper.Manage.FoldersTab
{
    /// <summary>
    /// Fills the folders tab with the buttons the assembly provides.
    /// </summary>
    internal static class FoldersTabButtonsLoader
    {
        internal static void Load()
        {
            ButtonListLoader.Load(
                ManageSettings.MainForm.FoldersTabPageBackgroundPanel,
                GetListOfSubClasses.Inherited.GetInterfaceImplementations<IFolderTabButtonData>());
        }
    }
}
