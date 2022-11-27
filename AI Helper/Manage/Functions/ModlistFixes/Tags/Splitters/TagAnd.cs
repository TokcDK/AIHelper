using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Manage.Functions.ModlistFixes.Data;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags.Splitters
{
    internal class TagAnd : ISplitterTag
    {
        public string Tag => "|and|";

        public bool IsTrue(ModlistFixesData data)
        {
            throw new NotImplementedException();
        }
    }
}
