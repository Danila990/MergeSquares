using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class ScriptableObjectHelpers
    {
        public static List<T> GetAllInstances<T>(string rootFolderPath) where T : ScriptableObject
        {
            var result = new List<T>();
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().StartsWith(rootFolderPath.ToLower()))
                    result.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }
#endif
            return result;
        }
    }
}