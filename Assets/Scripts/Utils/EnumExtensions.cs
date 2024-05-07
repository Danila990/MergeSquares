using System;
using System.Collections.Generic;

namespace Utils
{
    public static class EnumExtensions
    {
        public static int[] ToIntArray<T>(this T[] @enum) where T : Enum, IConvertible
        {
            var ints = new int[@enum.Length];

            for (int i = 0; i < @enum.Length; i++)
            {
                ints[i] = Convert.ToInt32(@enum[i]);
            }

            return ints;
        }

        public static int[] ToIntArray<T>(this List<T> @enum) where T : Enum, IConvertible
        {
            var ints = new int[@enum.Count];

            for (int i = 0; i < @enum.Count; i++)
            {
                ints[i] = Convert.ToInt32(@enum[i]);
            }

            return ints;
        }
    }
}