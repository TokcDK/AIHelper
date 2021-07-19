using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage
{
    static class ManageArrayExtensions
    {
        /// <summary>
        /// Trim each element of the array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        internal static string[] Trim(this string[] array)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }

            return array;
        }
    }
}
