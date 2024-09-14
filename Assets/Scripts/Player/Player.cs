public class Player
{
	#region Fields

	private string _name;
	private PlayerState _state;
	private PlayerToken _token;

	#endregion

	#region Properties

	public string Name => _name;
	public PlayerState State { get => _state; set => _state = value; }
	public PlayerToken Token => _token;
	public Tile CurrentTile => Token.CurrentTile;

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
