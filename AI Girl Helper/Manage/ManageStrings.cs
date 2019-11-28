using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.Utils
{
    class ManageStrings
    {
        /// <summary>
        /// Check if string A contains string B (if length of A > length of A with replaced B by "")<br></br><br></br>
        /// Speed check tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-check-if-a-string-occurs-within-a-string
        /// </summary>
        /// <param name="StringAWhereSearch"></param>
        /// <param name="StringBToSearch"></param>
        /// <returns></returns>
        public static bool IsStringAContainsStringB(string StringAWhereSearch, string StringBToSearch)
        {
            int StringAInWhichSearchLength = StringAWhereSearch.Length;
            if (StringAInWhichSearchLength > 0 && StringBToSearch.Length > 0)//safe check for empty values
            {
                //if string A contains string B then string A with replaced stringB by empty will be
                return StringAInWhichSearchLength > StringAWhereSearch.Replace(StringBToSearch, string.Empty).Length;
            }
            return false;

        }

        public static bool IsStringContainsAnyExclusion(string InputString, string[] exclusions)
        {
            if (InputString.Length > 0 && exclusions != null)
            {
                int exclusionsLength = exclusions.Length;
                for (int i = 0; i < exclusionsLength; i++)
                {
                    if (IsStringAContainsStringB(InputString, exclusions[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
