using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIHelper.Data.Modlist;
using AIHelper.Manage.Functions.ModlistFixes.Data;
using AIHelper.Manage.Functions.ModlistFixes.RulesParsers;

namespace AIHelper.Manage.Functions.ModlistFixes
{
    internal class ModlistFIxesForFlp : IFunctionForFlp
    {
        public string Symbol => "F";

        public string Description => T._("Autodetect and fix issues in the current profile's modlist");

        public void OnClick(object o, EventArgs e)
        {
            Apply();
        }

        private void Apply()
        {
            var modlist = new ModlistData();
            var loaders = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IModlistFixesRulesLoader>();
            foreach (var loader in loaders)
            {
                loader.LoadRules();
            }

            foreach(var mod in modlist.Mods)
            {
                var modDAta = new ModlistFixesData(mod);


            }
        }
    }
}
