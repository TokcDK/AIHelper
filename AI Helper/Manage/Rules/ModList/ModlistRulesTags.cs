namespace AIHelper.Manage.Rules.ModList
{
    internal class ModlistRulesTags
    {
        internal class Main : ModlistRulesTags
        {
            internal string Req = "req:";
            internal string Inc = "inc:";
        }

        internal class Sub : ModlistRulesTags
        {
            internal string File = "file:";
            internal string Or = "|or|";
            internal string And = "|and|";
        }
    }
}
