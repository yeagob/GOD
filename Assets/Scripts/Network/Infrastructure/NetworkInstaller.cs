using System;
using System.Collections.Generic;
using UnityEngine;
using Network.Services;
using Network.Repositories;
using Network.Models;
using Network.Presenters;

namespace Network.Infrastructure
{
    public class NetworkInstaller : MonoBehaviour
    {
        private static Dictionary<Type, object> _container = new Dictionary<Type, object>();

        void Awake()
        {
            InitializeServices();
            InitializeRepositories();
            InitializeModels();
            InitializePresenters();
        }

        private void InitializeServices()
        {
            RegisterSingleton<IFirebaseService>(new FirebaseService());
        }

        private void InitializeRepositories()
        {
            RegisterSingleton<IMatchRepository>(new MatchRepository(Resolve<IFirebaseService>()));
            RegisterSingleton<IPlayerMatchRepository>(new PlayerMatchRepository(Resolve<IFirebaseService>()));
            RegisterSingleton<IGameEventRepository>(new GameEventRepository(Resolve<IFirebaseService>()));
        }

        private void InitializeModels()
        {
            RegisterSingleton<IMatchModel>(new MatchModel(Resolve<IMatchRepository>()));
            RegisterSingleton<IPlayerMatchModel>(new PlayerMatchModel(Resolve<IPlayerMatchRepository>()));
            RegisterSingleton<IGameEventModel>(new GameEventModel(Resolve<IGameEventRepository>()));
        }

        private void InitializePresenters()
        {
            RegisterSingleton<IMatchPresenter>(new MatchPresenter(
                Resolve<IMatchModel>(), 
                Resolve<IPlayerMatchModel>(),
                Resolve<IGameEventModel>()));
                
            RegisterSingleton<IPlayerMatchPresenter>(new PlayerMatchPresenter(
                Resolve<IPlayerMatchModel>(),
                Resolve<IMatchModel>()));
                
            RegisterSingleton<IGameEventPresenter>(new GameEventPresenter(
                Resolve<IGameEventModel>(),
                Resolve<IPlayerMatchModel>(),
                Resolve<IMatchModel>()));
        }

        public static void RegisterSingleton<T>(T implementation)
        {
            _container[typeof(T)] = implementation;
        }

        public static T Resolve<T>()
        {
            if (_container.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            Debug.LogError($"Service of type {typeof(T)} not registered");
            return default;
        }
    }
}