using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundSystem : NetworkBehaviour
{
	[SerializeField] private float roundEndTime = 5f;

	public int orangePoints = 0;
	public int greenPoints = 0;

	private int greenLooseStreak = 0;
	private int orangeLooseStreak = 0;

	[Header("Money")]
	[SerializeField] private int winMoney = 0;
	[SerializeField] private int looseMoneyBase = 0;
	[SerializeField] private int looseMoneyScale = 0;
	[SerializeField] private int startMoney = 0;
	[SerializeField] private int maxMoney = 16000;

	[SyncVar]
	public int orangePlayers = 0;
	[SyncVar]
	public int greenPlayers = 0;

	[SyncVar]
	public string gameWinner = "";

	private SpectatorScreenController orangeRoom = null;
	private int OrangePlayers {
		get {
			if (orangeRoom != null) { return orangeRoom.players.Count; }
			orangeRoom = GameObject.Find("OrangeDeadRoom").GetComponent<SpectatorScreenController>();
			return orangeRoom.players.Count;
		}
	}

	private SpectatorScreenController greenRoom = null;
	private int GreenPlayers
	{
		get
		{
			if (greenRoom != null) { return greenRoom.players.Count; }
			greenRoom = GameObject.Find("GreenDeadRoom").GetComponent<SpectatorScreenController>();
			return greenRoom.players.Count;
		}
	}

	private UITimeManager timeManager;
	public UITimeManager TimeManager {
		get {
			if (timeManager != null) { return timeManager; }
			return timeManager = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UITimeManager>();
		}
	}

	private UIRoundManager roundManager;
	public UIRoundManager RoundManager
	{
		get
		{
			if (roundManager != null) { return roundManager; }
			return roundManager = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UIRoundManager>();
		}
	}

	private UIPlayerManager playerManager;
	public UIPlayerManager PlayerManager
	{
		get
		{
			if (playerManager != null) { return playerManager; }
			return playerManager = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UIPlayerManager>();
		}
	}

	private NetworkManagerLobby room;
	public NetworkManagerLobby Room
	{
		get
		{
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerLobby;
		}
	}

	private PlayerSpawnSystem spawnSystem;
	public PlayerSpawnSystem SpawnSystem
	{
		get
		{
			if (spawnSystem != null) { return spawnSystem; }
			return spawnSystem = GameObject.Find("SpawnSystem(Clone)").GetComponent<PlayerSpawnSystem>();
		}
	}
	
	public BombPlanted Bomb
	{
		get
		{
			return GameObject.Find("Bomb Planted(Clone)").GetComponent<BombPlanted>();
		}
	}

	private void Awake() => DontDestroyOnLoad(this.gameObject);

	#region Server
	public override void OnStartServer()
	{
		NetworkManagerLobby.OnServerStopped += CleanUpServer;
		NetworkManagerLobby.OnServerReadied += CheckToStartRound;
	}

	[ServerCallback]
	private void OnDestroy() => CleanUpServer();

	[Server]
	private void CleanUpServer() {
		NetworkManagerLobby.OnServerStopped -= CleanUpServer;
		NetworkManagerLobby.OnServerReadied -= CheckToStartRound;
	}

	[ServerCallback]
	public void StartRound() {
		TimeManager.StartRound();

		RpcStartRound();
	}

	[Server]
	private void CheckToStartRound(NetworkConnection conn) {
		if (!SceneManager.GetActiveScene().name.StartsWith("Map")) { return; }

		if (Room.GamePlayers.Count(x => x.connectionToClient.isReady) != Room.GamePlayers.Count) { return; }

		StartFreeze();
	}

	[Server]
	public void StartFreeze()
	{
		if (greenPoints + orangePoints == 15 || greenPoints + orangePoints == 0)
		{
			SetStartMoney();
		}

		orangePlayers = OrangePlayers;
		greenPlayers = GreenPlayers;

		PlayerManager.RpcSetText(orangePlayers, greenPlayers);

		TimeManager.StartFreeze(this);

		RpcStartCountDown();
	}

	[Server]
	public void EndRound(string winner) {
		if (winner == "O") {
			orangePoints++;
			RoundManager.RpcSetText(orangePoints, greenPoints);
			RoundManager.RpcShowWin("O");
			StartCoroutine(RoundEnding(winner));
			return;
		}
		if (winner == "G") {
			greenPoints++;
			RoundManager.RpcSetText(orangePoints, greenPoints);
			RoundManager.RpcShowWin("G");
			StartCoroutine(RoundEnding(winner));
			return;
		}
		RoundManager.RpcShowWin("D");
		StartCoroutine(RoundEnding(winner));
	}

	[Server]
	private IEnumerator RoundEnding(string winner) {
		yield return new WaitForSeconds(roundEndTime);
		if (orangePoints + greenPoints == 15)
		{
			int buffer = orangePoints;
			orangePoints = greenPoints;
			greenPoints = buffer;
			Room.SwitchTeams();
			RoundManager.RpcSetText(orangePoints, greenPoints);
		}
		GiveMoney(winner);
		if (winner == "O")
		{
			orangeLooseStreak = Mathf.Max(orangeLooseStreak - 1, 0);
			greenLooseStreak = Mathf.Min(greenLooseStreak + 1, 4);
		}
		if (winner == "G")
		{
			orangeLooseStreak = Mathf.Min(orangeLooseStreak + 1, 4);
			greenLooseStreak = Mathf.Max(greenLooseStreak - 1, 0);
		}
		if (orangePoints == 16) {
			GameWin("O");
			yield break;
		}
		if (greenPoints == 16) {
			GameWin("G");
			yield break;
		}
		if (orangePoints == 15 && greenPoints == 15) {
			GameWin("D");
			yield break;
		}
		RoundManager.RpcHideWin();
		Room.NewRound();
	}

	[ServerCallback]
	public void PlayerDied(string team) {
		if (team == "O") {
			orangePlayers--;
		}
		if (team == "G") {
			greenPlayers--;
		}
		CheckPlayersAlive();
		PlayerManager.RpcSetText(orangePlayers, greenPlayers);
	}

	[ServerCallback]
	private void CheckPlayersAlive() {
		if (TimeManager.isRound)
		{
			if (orangePlayers <= 0)
			{
				if (TimeManager.isBomb) { return; }
				TimeManager.EndRound("G");
				return;
			}
			if (greenPlayers <= 0)
			{
				TimeManager.EndRound("O");
				return;
			}
		}
	}
	
	[Server]
	public void PlantBomb() {
		TimeManager.PlantBomb();

		RpcPlantBomb();
	}

	[Server]
	public void DefuseBomb() {
		TimeManager.EndRound("G");
	}

	[ServerCallback]
	public void BombExplode() {
		Bomb.BombExplode();
		TimeManager.EndRound("O");
	}

	[Server]
	private void GameWin(string winner) {
		gameWinner = winner;
		Room.ServerChangeScene("GameEnd");
	}

	[Server]
	public void SetStartMoney() {
		foreach (NetworkGamePlayerLobby player in Room.GamePlayers) {
			player.money = startMoney;
		}
		SpawnSystem.SyncMoney();
	}

	[Server]
	private void GiveMoney(string winner) {
		foreach (NetworkGamePlayerLobby player in Room.GamePlayers) {
			if (player.IsGreen) {
				if (winner == "G") {
					player.money = Mathf.Min(player.money + winMoney, maxMoney);
					continue;
				}
				player.money = Mathf.Min(player.money + looseMoneyBase + looseMoneyScale * greenLooseStreak, maxMoney);
				continue;
			}
			if (player.IsOrange) {
				if (winner == "O") {
					player.money = Mathf.Min(player.money + winMoney, maxMoney);
					continue;
				}
				player.money = Mathf.Min(player.money + looseMoneyBase + looseMoneyScale * orangeLooseStreak, maxMoney);
				continue;
			}
		}
	}
	#endregion

	#region Client
	[ClientRpc]
	private void RpcStartCountDown() {
		foreach (string enable in ActionNames.Freeze)
		{
			InputManager.Add(enable);
		}

		TimeManager.StartFreeze(this);
	}

	[ClientRpc]
	private void RpcStartRound() {
		foreach (string disable in ActionNames.Freeze)
		{
			InputManager.Remove(disable);
		}

		TimeManager.StartRound();
	}

	[ClientRpc]
	private void RpcPlantBomb() {
		TimeManager.PlantBomb();
	}
	#endregion
}
