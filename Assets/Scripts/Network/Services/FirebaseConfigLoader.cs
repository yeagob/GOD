using UnityEngine;
using System.IO;
using Network.Models;

namespace Network.Services
{
    public static class FirebaseConfigLoader
    {
        private const string CONFIG_FILE_NAME = "firebase-config.json";
        
        public static FirebaseConfigData LoadConfig()
        {
            string configPath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
            
            try
            {
                if (File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    return JsonUtility.FromJson<FirebaseConfigData>(jsonContent);
                }
                else
                {
                    Debug.LogError($"Firebase config file not found at: {configPath}");
                    return CreateDefaultConfig();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load Firebase config: {ex.Message}");
                return CreateDefaultConfig();
            }
        }
        
        private static FirebaseConfigData CreateDefaultConfig()
        {
            return new FirebaseConfigData
            {
                databaseURL = "https://game-of-duck-multiplayer-default-rtdb.europe-west1.firebasedatabase.app/"
            };
        }
    }
}
