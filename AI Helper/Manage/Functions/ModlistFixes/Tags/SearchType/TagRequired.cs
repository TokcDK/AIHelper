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

        public bool IsFound(string ruleString)
        {
            throw new NotImplementedException();
        }

        public bool IsTrue(string ruleString)
        {
            throw new NotImplementedException();
        }
    }
}
