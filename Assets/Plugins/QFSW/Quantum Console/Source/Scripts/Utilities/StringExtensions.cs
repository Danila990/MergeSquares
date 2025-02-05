﻿using System;
using System.Linq;

namespace QFSW.QC.Utilities
{
    public static class StringExtensions
    {
        public static bool ContainsCaseInsensitive(this string source, string value)
        {
            if (string.IsNullOrEmpty(source))
            {
                if (string.IsNullOrEmpty(value)) { return true; }
                else { return false; }
            }

#if UNITY_WEBGL
            return source.ToLower().Contains(value.ToLower());
#else
            return source.Contains(value, StringComparison.OrdinalIgnoreCase);
#endif
        }

        public static bool Contains(this string source, string value, StringComparison comp)
        {
            return source?.IndexOf(value, comp) >= 0;
        }

        public static int CountFromIndex(this string source, char target, int index)
        {
            int count = 0;
            for (int i = index; i < source.Length; i++)
            {
                if (source[i] == target)
                {
                    count++;
                }
            }

            return count;
        }

        public static string ToUpper(this string source, int charNumber)
        {
            var temp = source.ToArray();

            for (var i = 0; i < source.Length; i++)
            {
                if (i < charNumber)
                {
                    continue;
                }
                
                temp[i] = char.ToUpper(temp[i]);
                break;
            }
            
            return new string(temp);
        }
    }
}