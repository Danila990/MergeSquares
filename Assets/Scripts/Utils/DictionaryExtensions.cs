using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary,
            Func<KeyValuePair<TKey, TValue>, bool> check,
            TValue def
        )
        {
            foreach (var pair in dictionary)
            {
                if (check(pair))
                {
                    return pair.Value;
                }
            }

            return def;
        }

        public static bool Get<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary,
            Func<KeyValuePair<TKey, TValue>, bool> check,
            out KeyValuePair<TKey, TValue>? returnValue)
        {
            foreach (var pair in dictionary)
            {
                if (check(pair))
                {
                    returnValue = pair;
                    return true;
                }
            }
            returnValue = null;
            return false;
        }

        public static bool Check<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary,
            Func<KeyValuePair<TKey, TValue>, bool> check
        )
        {
            var res = false;
            foreach (var pair in dictionary)
            {
                if (check(pair))
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

        public static TReturn Check<TKey, TValue, TReturn>(
            this IReadOnlyDictionary<TKey, TValue> dictionary,
            Func<TReturn, KeyValuePair<TKey, TValue>, TReturn> check,
            TReturn def
        )
        {
            TReturn res = def;
            foreach (var pair in dictionary)
            {
                res = check(res, pair);
            }

            return res;
        }

        public static TKey GetKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Value.Equals(value))
                {
                    return pair.Key;
                }
            }

            throw new Exception("Dictionary does not contain this value");
        }
    }
}