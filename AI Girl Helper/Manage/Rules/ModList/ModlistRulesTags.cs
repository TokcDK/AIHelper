namespace AIHelper.Manage.Rules.ModList
{
    internal class ModlistRulesTags
    {
        internal class Main : ModlistRulesTags
        {
            internal string REQ = "req:";
            internal string INC = "inc:";
        }

        internal class Sub : ModlistRulesTags
        {
            internal string File = "file:";
            internal string OR = "|or|";
            internal string AND = "|and|";
        }
    }
}
