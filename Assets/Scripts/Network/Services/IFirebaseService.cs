using System;
using System.Collections.Generic;

namespace Network.Services
{
    public interface IFirebaseService
    {
        void Initialize();
        void SetData<T>(string path, T data, Action<bool> callback = null);
        void GetData<T>(string path, Action<T> callback);
        void ListenForChanges<T>(string path, Action<T> callback);
        void StopListening(string path);
        string GenerateId(string path);
    }
}