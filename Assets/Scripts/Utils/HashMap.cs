using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public class HashMap<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<KeyValuePair> list = new List<KeyValuePair>();

        private Dictionary<TKey, int> _indexByKey = new Dictionary<TKey, int>();
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        [Serializable]
        private struct KeyValuePair
        {
            public TKey key;
            public TValue value;

            public KeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();
            _indexByKey.Clear();

            for (var i = 0; i < list.Count; i++)
            {
                var key = list[i].key;
                if (key != null && !ContainsKey(key))
                {
                    _dictionary.Add(key, list[i].value);
                    _indexByKey.Add(key, i);
                }
                else
                {
                    throw new SerializationException($"Key collision: {key}");
                }
            }
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value;

                if (_indexByKey.ContainsKey(key))
                {
                    var index = _indexByKey[key];
                    list[index] = new KeyValuePair(key, value);
                }
                else
                {
                    list.Add(new KeyValuePair(key, value));
                    _indexByKey.Add(key, list.Count - 1);
                }
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            list.Add(new KeyValuePair(key, value));
            _indexByKey.Add(key, list.Count - 1);
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
        public bool ContainsValue(TValue value) => _dictionary.ContainsValue(value);

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                var index = _indexByKey[key];
                list.RemoveAt(index);
                UpdateIndexes(index);
                _indexByKey.Remove(key);
                return true;
            }

            return false;
        }

        private void UpdateIndexes(int removedIndex)
        {
            for (var i = removedIndex; i < list.Count; i++)
            {
                var key = list[i].key;
                _indexByKey[key]--;
            }
        }

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public int Count => _dictionary.Count;
        public bool IsReadOnly { get; set; }

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            list.Clear();
            _indexByKey.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return _dictionary.TryGetValue(pair.Key, out var value) &&
                   EqualityComparer<TValue>.Default.Equals(value, pair.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();
            if (array.Length - arrayIndex < _dictionary.Count)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            foreach (var pair in _dictionary)
            {
                array[arrayIndex] = pair;
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            if (_dictionary.TryGetValue(pair.Key, out var value))
            {
                var valueMatch = EqualityComparer<TValue>.Default.Equals(value, pair.Value);
                if (valueMatch)
                {
                    return Remove(pair.Key);
                }
            }

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
    }
}