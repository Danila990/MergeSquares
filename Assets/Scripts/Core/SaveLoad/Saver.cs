using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.SaveLoad
{
    [Serializable]
    public class SaverData
    {
        public int version;
        public string data;
    }

    public class Saver : MonoBehaviour
    {
        [SerializeField] private int version;
        [SerializeField] private string key;

        public string Key => key;

        public Func<string> DataSaved;
        public Action<string, LoadContext> DataLoaded = (s, context) => {};
        public Action<LoadContext> DataLoadFinished = context => {};
        public Action<bool> SaveNeeded = (force) => {};

        public string Marshal<T>(T data)
        {
            var marshaledData = JsonUtility.ToJson(data, true);
            return JsonUtility.ToJson(new SaverData {version = version, data = marshaledData}, true);
        }

        public T Unmarshal<T>(string data, T def)
        {
            var saverData = JsonUtility.FromJson<SaverData>(data);
            if (saverData != null)
            {
                // TODO: maybe migrate saved data here to last version (migration in json format for each version)
                var unmarshaledData = JsonUtility.FromJson<T>(saverData.data);

                if (unmarshaledData == null)
                {
                    unmarshaledData = def;
                }
                
                return unmarshaledData;
            }

            return def;
        }
    }
}