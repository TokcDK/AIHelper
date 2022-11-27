using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Manage.Functions.ModlistFixes.Tags.SearchType;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags.SearchTarget
{
    internal class TagRequired : ISearchTypeTag
    {
        public string Tag => "req:";

        public bool IsTrue(ModlistFixesData data)
        {
            throw new NotImplementedException();
        }
    }
}
