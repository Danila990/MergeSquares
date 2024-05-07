using System;
using System.Collections.Generic;

namespace Utils
{
    [Serializable]
    public struct ListDictionaryItem<TKey, TData>
    {
        public TKey Key;
        public TData Value;

        public ListDictionaryItem(TKey key, TData value)
        {
            Key = key;
            Value = value;
        }
    }


    [Serializable]
    public class ListDictionary<TKey, TData>
    {
        public List<ListDictionaryItem<TKey, TData>> ListDictionaryUnits;


        public ListDictionary()
        {
            ListDictionaryUnits = new List<ListDictionaryItem<TKey, TData>>();
        }

        public ListDictionary(Dictionary<TKey, TData> data)
        {
            ListDictionaryUnits = new List<ListDictionaryItem<TKey, TData>>();

            if (data == null)
            {
                return;
            }

            foreach (TKey key in data.Keys)
            {
                ListDictionaryUnits.Add(new ListDictionaryItem<TKey, TData>(key, data[key]));
            }
        }

        public Dictionary<TKey, TData> ToDictionary()
        {
            Dictionary<TKey, TData> units = new Dictionary<TKey, TData>();

            foreach (ListDictionaryItem<TKey, TData> item in ListDictionaryUnits)
            {
                units.Add(item.Key, item.Value);
            }

            return units;
        }
    }
}