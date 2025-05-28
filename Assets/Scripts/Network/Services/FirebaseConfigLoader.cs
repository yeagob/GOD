using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using Network.Models;

namespace Network.Services
{
    public static class FirebaseConfigLoader
    {
        private const string CONFIG_FILE_NAME = "firebase-config.json";
        
        public static void LoadConfigAsync(System.Action<FirebaseConfigData> callback)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                LoadConfigWebGL(callback);
            }
            else
            {
                LoadConfigLocal(callback);
            }
        }
        
        private static void LoadConfigWebGL(System.Action<FirebaseConfigData> callback)
        {
            string configPath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
            
            UnityEngine.MonoBehaviour coroutineRunner = UnityEngine.Object.FindObjectOfType<UnityEngine.MonoBehaviour>();
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(LoadConfigCoroutine(configPath, callback));
            }
            else
            {
                Debug.LogError("No MonoBehaviour found to run coroutine");
                callback?.Invoke(CreateDefaultConfig());
            }
        }
        
        private static IEnumerator LoadConfigCoroutine(string configPath, System.Action<FirebaseConfigData> callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(configPath))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonContent = request.downloadHandler.text;
                        FirebaseConfigData config = JsonUtility.FromJson<FirebaseConfigData>(jsonContent);
                        callback?.Invoke(config);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to parse Firebase config: {ex.Message}");
                        callback?.Invoke(CreateDefaultConfig());
                    }
                }
                else
                {
                    Debug.LogError($"Failed to load Firebase config: {request.error}");
                    callback?.Invoke(CreateDefaultConfig());
                }
            }
        }
        
        private static void LoadConfigLocal(System.Action<FirebaseConfigData> callback)
        {
            string configPath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
            
            try
            {
                if (File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    FirebaseConfigData config = JsonUtility.FromJson<FirebaseConfigData>(jsonContent);
                    callback?.Invoke(config);
                }
                else
                {
                    Debug.LogError($"Firebase config file not found at: {configPath}");
                    callback?.Invoke(CreateDefaultConfig());
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load Firebase config: {ex.Message}");
                callback?.Invoke(CreateDefaultConfig());
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
