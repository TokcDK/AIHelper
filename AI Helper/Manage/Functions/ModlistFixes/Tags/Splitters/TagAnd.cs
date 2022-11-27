using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags.Splitters
{
    internal class TagAnd : ISplitterTag
    {
        public string Tag => "|and|";

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
