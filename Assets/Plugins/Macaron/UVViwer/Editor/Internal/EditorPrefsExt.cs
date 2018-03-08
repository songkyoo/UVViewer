using System;
using UnityEngine;
using UnityEditor;

namespace Macaron.UVViewer.Editor.Internal
{
    public static class EditorPrefsExt
    {
        public static Color GetColor(string key, Color defaultValue)
        {
            Color color;
            string defaultText = ColorUtility.ToHtmlStringRGBA(defaultValue);

            if (!ColorUtility.TryParseHtmlString('#' + EditorPrefs.GetString(key, defaultText), out color))
            {
                color = defaultValue;
            }

            return color;
        }

        public static Color GetColor(string key)
        {
            return GetColor(key, Color.white);
        }

        public static void SetColor(string key, Color value)
        {
            EditorPrefs.SetString(key, ColorUtility.ToHtmlStringRGBA(value));
        }

        public static T GetEnum<T>(string key, T defaultValue = default(T)) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException();
            }

            string defaultText = defaultValue.ToString();
            string text = EditorPrefs.GetString(key, defaultText);

            try
            {
                return (T)Enum.Parse(typeof(T), text);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static void SetEnum<T>(string key, T value) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException();
            }

            EditorPrefs.SetString(key, value.ToString());
        }

        public static T GetAsset<T>(string key, T defaultValue = null) where T : UnityEngine.Object
        {
            string guid = EditorPrefs.GetString(key, string.Empty);

            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }

        public static void SetAsset<T>(string key, T value) where T : UnityEngine.Object
        {
            string path = AssetDatabase.GetAssetPath(value);

            if (string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString(key, string.Empty);
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString(key, guid);
        }
    }
}
