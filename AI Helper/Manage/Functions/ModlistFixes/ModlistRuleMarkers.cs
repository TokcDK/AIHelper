using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage.Functions.ModlistFixes
{
    internal class ModlistFixesRuleTags
    {
        /// <summary>
        /// The splitter between rule records means must be apply any part of the rule
        /// </summary>
        internal static string Or { get; } = "|or|";
        /// <summary>
        /// The splitter between rule records means must be apply all parts of the rule
        /// </summary>
        internal static string And { get; } = "|and|";
        /// <summary>
        /// The prefix means search file
        /// </summary>
        internal static string File { get; } = "file:";
        /// <summary>
        /// The search prefix means Required specified Mod dir name or File subpath in mod
        /// </summary>
        internal static string Req { get; } = "req:";
        /// <summary>
        /// The search prefix means Incompatible with specified Mod dir name or File subpath in mod
        /// </summary>
        internal static string Inc { get; } = "inc:";
    }
}
