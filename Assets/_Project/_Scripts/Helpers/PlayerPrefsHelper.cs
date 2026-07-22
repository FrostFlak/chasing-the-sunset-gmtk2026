using UnityEngine;

namespace Helpers {
    public static class PlayerPrefsHelper {
        
        public static bool GetBool(string key) => PlayerPrefs.GetInt(key, 1) == 1;

        public static void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}