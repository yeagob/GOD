using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;

namespace Network.Services
{
    public class FirebaseService : IFirebaseService
    {
        private FirebaseDatabase _database;
        private bool _isInitialized = false;
        private Dictionary<string, ValueChangedEventHandler> _listeners = new Dictionary<string, ValueChangedEventHandler>();

        public async void Initialize()
        {
            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                
                if (dependencyStatus == DependencyStatus.Available)
                {
                    var app = FirebaseApp.DefaultInstance;
                    _database = FirebaseDatabase.DefaultInstance;
                    _database.SetPersistenceEnabled(false);
                    _isInitialized = true;
                    Debug.Log("Firebase initialized successfully");
                }
                else
                {
                    Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Firebase initialization failed: {ex.Message}");
            }
        }

        public void SetData<T>(string path, T data, Action<bool> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized");
                callback?.Invoke(false);
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(data);
                _database.GetReference(path).SetRawJsonValueAsync(json).ContinueWith(task => 
                {
                    bool success = !task.IsFaulted && !task.IsCanceled;
                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Firebase SetData failed: {task.Exception}");
                    }
                    else
                    {
                        Debug.Log($"Firebase: Data set successfully at path {path}");
                    }
                    callback?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting data: {ex.Message}");
                callback?.Invoke(false);
            }
        }

        public void GetData<T>(string path, Action<T> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized");
                callback?.Invoke(default);
                return;
            }

            try
            {
                _database.GetReference(path).GetValueAsync().ContinueWith(task => 
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError($"Firebase GetData failed: {task.Exception}");
                        callback?.Invoke(default);
                        return;
                    }

                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        try
                        {
                            string json = snapshot.GetRawJsonValue();
                            Debug.Log($"Firebase: Retrieved data from {path}: {json}");
                            
                            if (typeof(T) == typeof(Dictionary<string, object>))
                            {
                                var dict = new Dictionary<string, object>();
                                foreach (var child in snapshot.Children)
                                {
                                    dict[child.Key] = child.GetRawJsonValue();
                                }
                                callback?.Invoke((T)(object)dict);
                            }
                            else
                            {
                                T result = JsonUtility.FromJson<T>(json);
                                callback?.Invoke(result);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error deserializing data: {ex.Message}");
                            callback?.Invoke(default);
                        }
                    }
                    else
                    {
                        Debug.Log($"Firebase: No data exists at path {path}");
                        callback?.Invoke(default);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting data: {ex.Message}");
                callback?.Invoke(default);
            }
        }

        public void ListenForChanges<T>(string path, Action<T> callback)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized");
                return;
            }

            try
            {
                ValueChangedEventHandler listener = (sender, args) => 
                {
                    if (args.DatabaseError != null)
                    {
                        Debug.LogError($"Firebase listener error at {path}: {args.DatabaseError.Message}");
                        return;
                    }

                    if (args.Snapshot.Exists)
                    {
                        try
                        {
                            string json = args.Snapshot.GetRawJsonValue();
                            T result = JsonUtility.FromJson<T>(json);
                            callback?.Invoke(result);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in listener callback: {ex.Message}");
                        }
                    }
                };

                _database.GetReference(path).ValueChanged += listener;
                _listeners[path] = listener;
                Debug.Log($"Firebase: Started listening at path {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting up listener: {ex.Message}");
            }
        }

        public void StopListening(string path)
        {
            if (!_isInitialized)
            {
                return;
            }

            try
            {
                if (_listeners.ContainsKey(path))
                {
                    _database.GetReference(path).ValueChanged -= _listeners[path];
                    _listeners.Remove(path);
                    Debug.Log($"Firebase: Stopped listening at path {path}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping listener: {ex.Message}");
            }
        }

        public string GenerateId(string path)
        {
            if (!_isInitialized)
            {
                Debug.LogError("Firebase not initialized");
                return Guid.NewGuid().ToString();
            }

            try
            {
                string id = _database.GetReference(path).Push().Key;
                Debug.Log($"Firebase: Generated ID {id} for path {path}");
                return id;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error generating ID: {ex.Message}");
                return Guid.NewGuid().ToString();
            }
        }
    }
}
