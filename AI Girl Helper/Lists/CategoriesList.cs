using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper
{
    internal class CategoriesList
    {
        internal string ID;
        internal string Name;
        internal string NexusID;
        internal string ParentID;
        internal CategoriesList(string id, string name, string nID, string pID)
        {
            ID = id;
            Name = name;
            NexusID = nID;
            ParentID = pID;
        }
    }
}
