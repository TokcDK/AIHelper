using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Manage.Functions.ModlistFixes.Data;

namespace AIHelper.Manage.Functions.ModlistFixes.Tags.SearchTarget
{
    internal class TagFile : ISearchTargetPrefixTag
    {
        public string Tag => "file:";

        public bool Found(string path) { return File.Exists(path); }

        public bool IsTrue(ModlistFixesData data)
        {
            return true;
        }
    }
}
