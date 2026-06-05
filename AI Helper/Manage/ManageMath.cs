using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Manage
{
    internal static class ManageMath
    {
        /// <summary>
        /// Clamp method to restrict a <paramref name="value"/> within a specified range from <paramref name="min"/> to <paramref name="max"/>. 
        /// </summary>
        /// <param name="value">The value to be clamped.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns>If the <paramref name="value"/> is less than the <paramref name="min"/>, it returns the <paramref name="min"/>; 
        /// if it's greater than the <paramref name="max"/>, it returns the <paramref name="max"/>; 
        /// otherwise, it returns the <paramref name="value"/> itself.</returns>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }
    }
}
