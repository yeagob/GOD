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
        private bool _isInitialized = false;

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Debug.Log($"Firebase service initialized with URL: {DATABASE_URL}");
                StartCoroutine(TestConnection());
            }
        }

        public void SetData<T>(string path, T data, Action<bool> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase service not initialized!");
                callback?.Invoke(false);
                return;
            }

            string json = JsonUtility.ToJson(data);
            Debug.Log($"Firebase: Setting data at {path} with JSON: {json}");
            StartCoroutine(SetDataCoroutine(path, json, callback));
        }

        public void GetData<T>(string path, Action<T> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase service not initialized!");
                callback?.Invoke(default);
                return;
            }

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

        private IEnumerator TestConnection()
        {
            string testUrl = $"{DATABASE_URL}.json";
            using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Firebase: Connection test successful");
                }
                else
                {
                    Debug.LogError($"Firebase: Connection test failed - {request.error}");
                }
            }
        }

        private IEnumerator SetDataCoroutine(string path, string json, Action<bool> callback)
        {
            string url = $"{DATABASE_URL}{path}.json";
            Debug.Log($"Firebase: PUT to {url}");
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                yield return request.SendWebRequest();

                bool success = request.result == UnityWebRequest.Result.Success;
                
                if (success)
                {
                    Debug.Log($"Firebase: Data set successfully at path {path}");
                    Debug.Log($"Firebase: Response: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.LogError($"Firebase: Failed to set data at {path}");
                    Debug.LogError($"Firebase: Error: {request.error}");
                    Debug.LogError($"Firebase: Response Code: {request.responseCode}");
                    if (request.downloadHandler != null)
                    {
                        Debug.LogError($"Firebase: Response: {request.downloadHandler.text}");
                    }
                }
                
                callback?.Invoke(success);
            }
        }

        private IEnumerator GetDataCoroutine<T>(string path, Action<T> callback)
        {
            string url = $"{DATABASE_URL}{path}.json";
            Debug.Log($"Firebase: GET from {url}");
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"Firebase: Raw response from {path}: {responseText}");
                    
                    if (!string.IsNullOrEmpty(responseText) && responseText != "null")
                    {
                        try
                        {
                            T result = JsonUtility.FromJson<T>(responseText);
                            callback?.Invoke(result);
                            Debug.Log($"Firebase: Data retrieved and parsed from path {path}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Firebase: Failed to parse JSON from {path}: {e.Message}");
                            Debug.LogError($"Firebase: JSON was: {responseText}");
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
                    Debug.LogError($"Firebase: Response Code: {request.responseCode}");
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

        private void OnDestroy()
        {
            _listeners.Clear();
        }
    }
}
