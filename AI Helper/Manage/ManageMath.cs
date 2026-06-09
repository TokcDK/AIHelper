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

        /// <summary>
        /// Calculates the aspect ratio of a given width and height by finding their greatest common divisor (GCD) and returning the ratio in the format "width:height".
        /// </summary>
        /// <param name="w">The width.</param>
        /// <param name="h">The height.</param>
        /// <returns>The aspect ratio in the format "width:height", "16:9" for example.</returns>
        internal static object GetAspectRatio(int w, int h)
        {
            int gcd = GCD(w, h);
            return $"{w / gcd}:{h / gcd}";
        }

        /// <summary>
        /// Calculates the greatest common divisor (GCD) of two integers using the Euclidean algorithm.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns>The greatest common divisor of the two integers.</returns>
        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}
