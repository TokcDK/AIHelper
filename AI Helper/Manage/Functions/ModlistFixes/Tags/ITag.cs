using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Manage.Functions.ModlistFixes.Data;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags
{
    internal interface ITag
    {
        string Tag { get; }
        bool IsTrue(ModlistFixesData data);
    }
}
