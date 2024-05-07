using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Utils
{
    public static class ListExtensions
    {
        public static T PickRandom<T>( this IReadOnlyList<T> list, LCRandom rnd ) {
            if ( list.Count <= 0 ) {
                throw new Exception( $"[Extensions][PickRandom] List couldn't be empty." );
            }

            var idx = rnd.Range32( 0, list.Count );
            return list[ idx ];
        }
        
        public static T GetRandom<T>(this List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static bool TryGetNonRepeatingRandom<T>(this List<T> enumerable, int count, out List<T> result)
        {
            result = new List<T>();
            
            if (enumerable.HasNonRepeating(count, out var distinct))
            {
                distinct = distinct.Shuffle();
                result.AddRange(distinct.Take(count));

                return true;
            }

            return false;
        }
        
        public static List<T> Shuffle<T>(this List<T> enumerable)
        {
            var random = new Random();
            return enumerable.Shuffle(random);
        }
        
        public static List<T> Shuffle<T>(this List<T> enumerable, Random random)
        {
            return enumerable.OrderBy(value => random.Next()).ToList();
        }
        
        private static bool HasNonRepeating<T>(this List<T> enumerable, int count, out List<T> distinct)
        {
            return HasNonRepeating(enumerable, count, value => value.Distinct().ToList(), out distinct);
        }
        
        private static bool HasNonRepeating<T>(this List<T> enumerable, int count, Func<List<T>, List<T>> distinctFunc, out List<T> distinct)
        {
            distinct = distinctFunc.Invoke(enumerable);
            
            if (count > enumerable.Count) return false;

            var isCorrect = count <= distinct.Count;
            return isCorrect;
        }
        
        public static T GetBy<T>(this List<T> list, Func<T, bool> condition, T defaultValue = default)
        {
            foreach (var item in list)
            {
                if (condition(item))
                    return item;
            }

            return defaultValue;
        }

        public static List<T> GetListBy<T>(this List<T> list, Func<T, bool> condition, T defaultValue = default)
        {
            var result = new List<T>();
            foreach (var item in list)
            {
                if (condition(item)) result.Add(item);
            }

            return result;
        }

        public static bool TryWeightRandom<T>(this List<T> list, Func<T, float> getWeight, out T result)
        {
            var res = false;
            result = list[0];

            var totalWeight = 0f;
            foreach (var item in list)
                totalWeight += getWeight(item);

            var random = UnityEngine.Random.Range(0f, totalWeight);
            var accumulatedValue = 0f;
            foreach (var item in list)
            {
                var weight = getWeight(item);
                accumulatedValue += weight;
                if (accumulatedValue >= random)
                {
                    result = item;
                    res = true;
                    break;
                }
            }

            return res;
        }

        public static float WeightToPercentage<T>(this List<T> list, Func<T, float> getWeight, float convertedWeight)
        {
            var totalWeight = list.Sum(getWeight);
            return convertedWeight * 100 / totalWeight;
        }

        public static bool SwapErase<T>(this IList<T> list, T el)
        {
            return SwapEraseAt(list, list.IndexOf(el));
        }

        public static bool SwapEraseAt<T>(this IList<T> list, int index)
        {
            if (0 <= index && index < list.Count)
            {
                list[index] = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return true;
            }

            return false;
        }

        public static List<T> GetSorted<T>(this List<T> list)
        {
            var newList = new List<T>(list);
            newList.Sort();

            return newList;
        }
        
        public static List<T2> Map<T1, T2>(this IReadOnlyList<T1> list, Func<T1, T2> mapper)
        {
            var res = new List<T2>();

            foreach (var value in list)
            {
                res.Add(mapper(value));
            }
            
            return res;
        }
        
        public static List<T> GetRangeToEnd<T>(this List<T> list, int start) 
        {
            return list.GetRange(start, list.Count - start);
        }
    }
}