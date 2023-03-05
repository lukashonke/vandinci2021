using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Text Database", menuName = "Gama/Configuration/Text Database")]
    public class TextDatabase : SerializedScriptableObject
    {
        public Dictionary<string, TextData> texts;
        
        public TextData GetText(string key)
        {
            if (texts.TryGetValue(key, out var text))
            {
                return text;
            }

            return null;
        }
    }
    
    [Serializable]
    public class TextData
    {
        public string title;
        public string text;
        public Sprite sprite;
    }
}