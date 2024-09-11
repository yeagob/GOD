public class Player
{
	#region Fields

	private string _name;
	private PlayerState _state;
	private PlayerToken _token;
	private Tile _currentTile;

	#endregion

	#region Properties

	public string Name => _name;
	public PlayerState State => _state;
	public PlayerToken Token => _token;
	public Tile CurrentTile
	{
		get => _currentTile;
		set => _currentTile = value;
	}

	#endregion

	#region Public Methods

	public Player(string name, PlayerToken token)
	{
		_name = name;
		_token = token;
		_state = PlayerState.Waiting;
	}

	#endregion
}
