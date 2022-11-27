using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags.SearchTarget
{
    internal class TagFile : ISearchTargetPrefixTag
    {
        public string Tag => "file:";

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
