using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIHelper.Manage.ManageModOrganizer.CustomExecutables;

namespace AIHelper.Manage.MOiniCustomExeFixers
{
    public class CustomExeFixData
    {
        internal string Path;
        internal string Attribute;
        internal CustomExecutable CustomExeData;
    }

    interface ICustomExePathFixerBase
    {
        void TryFix(CustomExeFixData data);
    }
}
