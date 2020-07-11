﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage
{
    internal static class ManageNumbersExtensions
    {
        /// <summary>
        /// If the number is Odd
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }
    }
}
