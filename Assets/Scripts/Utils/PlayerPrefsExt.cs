using UnityEngine;

namespace Utils
{
    public static class PlayerPrefsExt
    {
        public static bool GetBool(string key, bool def)
        {
            return PlayerPrefs.GetInt(key, def ? 1 : 0) == 1;
        }
        
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static void Save() => PlayerPrefs.Save();
    }
}