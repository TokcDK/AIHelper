using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags
{
    internal interface ITag
    {
        string Tag { get; }
        bool IsFound(string ruleString);
        bool IsTrue(string ruleString);
    }
}
