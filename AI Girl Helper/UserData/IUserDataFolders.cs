using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.UserData
{
    public interface IUserDataFolders
    {
        string Foldername();

        string TargetFolderSuffix();

        string Extension();

        string TypeFolder();

        string TargetFolderName();

    }
}
