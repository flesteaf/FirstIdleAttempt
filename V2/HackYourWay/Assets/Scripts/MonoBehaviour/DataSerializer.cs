using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class DataSerializer : MonoBehaviour
    {
        public static bool SaveString(string key, string value)
        {
            try
            {
                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
                return true;
            }
            catch (PlayerPrefsException ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        public static string LoadString(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            return null;
        }

        internal static void ClearKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}