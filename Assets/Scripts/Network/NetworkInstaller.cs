using UnityEngine;

public class NetworkInstaller : MonoBehaviour
{
    [SerializeField] private FirebaseService firebaseService;
    
    private MatchRepository matchRepository;
    private PlayerMatchRepository playerMatchRepository;
    private GameEventRepository gameEventRepository;
    
    private MatchModel matchModel;
    private PlayerMatchModel playerMatchModel;
    private GameEventModel gameEventModel;
    
    private MatchPresenter matchPresenter;
    private PlayerMatchPresenter playerMatchPresenter;
    private GameEventPresenter gameEventPresenter;
    
    private void Awake()
    {
        InstallDependencies();
    }
    
    private void InstallDependencies()
    {
        if (firebaseService == null)
        {
            firebaseService = FindObjectOfType<FirebaseService>();
            if (firebaseService == null)
            {
                firebaseService = gameObject.AddComponent<FirebaseService>();
            }
        }
        
        matchRepository = new MatchRepository(firebaseService);
        playerMatchRepository = new PlayerMatchRepository(firebaseService);
        gameEventRepository = new GameEventRepository(firebaseService);
        
        matchModel = new MatchModel(matchRepository);
        playerMatchModel = new PlayerMatchModel(playerMatchRepository);
        gameEventModel = new GameEventModel(gameEventRepository);
        
        matchPresenter = gameObject.AddComponent<MatchPresenter>();
        playerMatchPresenter = gameObject.AddComponent<PlayerMatchPresenter>();
        gameEventPresenter = gameObject.AddComponent<GameEventPresenter>();
        
        var matchPresenterConstructor = typeof(MatchPresenter).GetConstructor(new[] { typeof(MatchModel), typeof(PlayerMatchModel), typeof(GameEventModel) });
        if (matchPresenterConstructor != null)
        {
            matchPresenterConstructor.Invoke(matchPresenter, new object[] { matchModel, playerMatchModel, gameEventModel });
        }
        
        var playerMatchPresenterConstructor = typeof(PlayerMatchPresenter).GetConstructor(new[] { typeof(PlayerMatchModel), typeof(MatchModel) });
        if (playerMatchPresenterConstructor != null)
        {
            playerMatchPresenterConstructor.Invoke(playerMatchPresenter, new object[] { playerMatchModel, matchModel });
        }
        
        var gameEventPresenterConstructor = typeof(GameEventPresenter).GetConstructor(new[] { typeof(GameEventModel), typeof(PlayerMatchModel), typeof(MatchModel) });
        if (gameEventPresenterConstructor != null)
        {
            gameEventPresenterConstructor.Invoke(gameEventPresenter, new object[] { gameEventModel, playerMatchModel, matchModel });
        }
    }
    
    public MatchPresenter GetMatchPresenter() => matchPresenter;
    public PlayerMatchPresenter GetPlayerMatchPresenter() => playerMatchPresenter;
    public GameEventPresenter GetGameEventPresenter() => gameEventPresenter;
    public FirebaseService GetFirebaseService() => firebaseService;
}
