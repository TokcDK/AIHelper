namespace AIHelper
{
    internal class CategoriesList
    {
        internal string Id;
        internal string Name;
        internal string NexusId;
        internal string ParentId;
        internal CategoriesList(string id, string name, string nId, string pId)
        {
            Id = id;
            Name = name;
            NexusId = nId;
            ParentId = pId;
        }
    }
}
