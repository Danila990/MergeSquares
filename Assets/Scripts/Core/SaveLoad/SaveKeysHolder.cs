using System.Collections.Generic;
using UnityEngine;

namespace Core.SaveLoad
{
    public class SaveKeysHolderData
    {
        public List<string> keys;
        
    }
    public class SaveKeysHolder
    {
        public SaveKeysHolderData Data { get; }
        
        private readonly string _key;

        public SaveKeysHolder(string key, List<string> defaultSaveKeys = null)
        {
            _key = key;
            var data = PlayerPrefs.GetString(_key);
            Data = JsonUtility.FromJson<SaveKeysHolderData>(data) ?? new SaveKeysHolderData{keys = defaultSaveKeys};
        }
        
        public bool IsSaveExist(string key)
        {
            return Data.keys.Contains(key);
        }

        public void AddKey(string key)
        {
            if (!IsSaveExist(key))
            {
                Data.keys.Add(key);
                Save();
            }
        }
        
        public void RemoveKey(string key)
        {
            if (IsSaveExist(key))
            {
                Data.keys.Remove(key);
                Save();
            }
        }
        
        private void Save()
        {
            var data = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(_key, data);
        }
    }
}