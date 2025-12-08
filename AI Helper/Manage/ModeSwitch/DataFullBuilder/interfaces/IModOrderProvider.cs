using System.Collections.Generic;

namespace AIHelper.Manage.ModeSwitch.DataFullBuilder.Interfaces
{
    public interface IModOrderProvider
    {
        IReadOnlyList<string> GetOrderedModFolders();
    }
}
