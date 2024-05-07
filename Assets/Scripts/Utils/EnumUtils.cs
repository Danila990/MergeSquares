using System;
using System.Collections.Generic;

namespace Utils
{
    public static class EnumUtils
    {
        public static List<T> GetValues<T>() where T : Enum
        {
            var list = new List<T>();
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
                list.Add((T) value);
            return list;
        }
    }
}