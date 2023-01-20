using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization;
using UnityEngine;

namespace _Chi.Scripts.Persistence
{
    public static class PersistenceUtils
    {
        public static string GetDefaultSaveName()
        {
            return Application.persistentDataPath + "/save.dat";
        }
        
        public static void SaveState(string filePath, PlayerProgressData data)
        {
            byte[] bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary);
            File.WriteAllBytes(filePath, bytes);
        }
        
        public static PlayerProgressData LoadState(string filePath)
        {
            if (!File.Exists(filePath)) return null;
	
            byte[] bytes = File.ReadAllBytes(filePath);
            var data = SerializationUtility.DeserializeValue<PlayerProgressData>(bytes, DataFormat.Binary);
            return data;
        }

        public static void ResetState(string filePath)
        {
            File.Delete(filePath);
        }
    }
}