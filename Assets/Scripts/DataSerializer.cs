using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class DataSerializer : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public static bool SaveString(string key, string value)
        {
            try
            {
                PlayerPrefs.SetString(key, value);
                return true;
            }
            catch (PlayerPrefsException)
            {
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

        public static void SaveValues()
        {
            PlayerPrefs.Save();
        }
    }
}