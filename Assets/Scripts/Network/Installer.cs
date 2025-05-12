using UnityEngine;

namespace Network
{
    public class Installer : MonoBehaviour
    {
        private DependencyManager _dependencyManager;

        void Awake()
        {
            _dependencyManager = new DependencyManager();

            // Register Services
            //_dependencyManager.Register<IFirebaseDatabaseService, FirebaseDatabaseService>(new FirebaseDatabaseService());

            // Note: Presenters (LobbyPresenter, MatchPresenter) are typically created
            // by a higher-level manager or scene controller when needed, resolving
            // their dependencies from the DependencyManager.
            // Example (you would do this in a LobbySceneManager or MatchSceneManager script):
            // IFirebaseDatabaseService dbService = _dependencyManager.Resolve<IFirebaseDatabaseService>();
            // LobbyPresenter lobbyPresenter = new LobbyPresenter(dbService);
            // MatchPresenter matchPresenter = new MatchPresenter(dbService, currentMatchId, localPlayerId);
        }

        public T Resolve<T>()
        {
            return _dependencyManager.Resolve<T>();
        }

        void OnDestroy()
        {
            _dependencyManager.Clear();
            // Ensure all disposable presenters are disposed of
            // Example: _lobbyPresenter?.Dispose(); _matchPresenter?.Dispose();
        }
    }
}