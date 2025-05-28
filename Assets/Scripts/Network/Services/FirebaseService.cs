using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

namespace Network.Services
{
    public class FirebaseService : MonoBehaviour, IFirebaseService
    {
        private const string DATABASE_URL = "https://game-of-duck-multiplayer-default-rtdb.europe-west1.firebasedatabase.app/";
        private Dictionary<string, object> _listeners = new Dictionary<string, object>();

        public void Initialize()
        {
            Debug.Log("Firebase service initialized with URL: " + DATABASE_URL);
        }

        public void SetData<T>(string path, T data, Action<bool> callback = null)
        {
            string json = JsonUtility.ToJson(data);
            StartCoroutine(SetDataCoroutine(path, json, callback));
        }

        public void GetData<T>(string path, Action<T> callback)
        {
            StartCoroutine(GetDataCoroutine<T>(path, callback));
        }

        public void ListenForChanges<T>(string path, Action<T> callback)
        {
            _listeners[path] = callback;
            StartCoroutine(ListenForChangesCoroutine<T>(path, callback));
        }

        public void StopListening(string path)
        {
            if (_listeners.ContainsKey(path))
            {
                _listeners.Remove(path);
                Debug.Log($"Firebase: Stopped listening at path {path}");
            }
        }

        public string GenerateId(string path)
        {
            string id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);
            Debug.Log($"Firebase: Generated ID {id} for path {path}");
            return id;
        }

        private IEnumerator SetDataCoroutine(string path, string json, Action<bool> callback)
        {
            string url = $"{DATABASE_URL}{path}.json";
            
            using (UnityWebRequest request = UnityWebRequest.Put(url, json))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();

                bool success = request.result == UnityWebRequest.Result.Success;
                
                if (success)
                {
                    Debug.Log($"Firebase: Data set successfully at path {path}");
                }
                else
                {
                    Debug.LogError($"Firebase: Failed to set data at {path}: {request.error}");
                }
                
                callback?.Invoke(success);
            }
        }

        private IEnumerator GetDataCoroutine<T>(string path, Action<T> callback)
        {
            string url = $"{DATABASE_URL}{path}.json";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    
                    if (!string.IsNullOrEmpty(responseText) && responseText != "null")
                    {
                        try
                        {
                            T result = JsonUtility.FromJson<T>(responseText);
                            callback?.Invoke(result);
                            Debug.Log($"Firebase: Data retrieved from path {path}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Firebase: Failed to parse JSON from {path}: {e.Message}");
                            callback?.Invoke(default);
                        }
                    }
                    else
                    {
                        Debug.Log($"Firebase: No data found at path {path}");
                        callback?.Invoke(default);
                    }
                }
                else
                {
                    Debug.LogError($"Firebase: Failed to get data from {path}: {request.error}");
                    callback?.Invoke(default);
                }
            }
        }

        private IEnumerator ListenForChangesCoroutine<T>(string path, Action<T> callback)
        {
            while (_listeners.ContainsKey(path))
            {
                yield return GetDataCoroutine<T>(path, callback);
                yield return new WaitForSeconds(2f);
            }
        }
    }
}
