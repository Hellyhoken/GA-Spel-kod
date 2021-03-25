using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager
{
	[SerializeField] private int minPlayers = 2;
	private Scene MenuScene {
		get {
			return SceneManager.GetSceneByName("Menu");
		}
	}

	[Header("Room")]
	[SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab = null;

	[Header("Game")]
	[SerializeField] private NetworkGamePlayerLobby gamePlayerPrefab = null;
	[SerializeField] private GameObject playerSpawnSystem = null;
	[SerializeField] public GameObject roundSystem = null;
	[SerializeField] public GameObject publicUI = null;

	public GameObject[] players = new GameObject[10];

	private string previousSceneName = string.Empty;

	private GameObject publicUIInstance = null;
	private GameObject playerSpawnSystemInstance = null;
	private GameObject roundSystemInstance = null;

	private RoundSystem roundSystemInstance2 = null;

	public static event Action OnClientConnected;
	public static event Action OnClientDisconnected;
	public static event Action<NetworkConnection> OnServerReadied;
	public static event Action OnServerStopped;

	public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();
	public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>();

	
	
	public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

	public override void OnStartClient()
	{
		var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

		foreach (var prefab in spawnablePrefabs) {
			ClientScene.RegisterPrefab(prefab);
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		OnClientConnected?.Invoke();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		OnClientDisconnected?.Invoke();
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		if (numPlayers >= maxConnections) {
			conn.Disconnect();
			return;
		}
		
		if (SceneManager.GetActiveScene().name != MenuScene.name) {
			conn.Disconnect();
			return;
		}
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		if (SceneManager.GetActiveScene().name == MenuScene.name) {
			bool isLeader = RoomPlayers.Count == 0;

			NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

			roomPlayerInstance.IsLeader = isLeader;

			NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		if (conn != null) {
			var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

			RoomPlayers.Remove(player);

			NotifyPlayersOfReadyState();
		}

		base.OnServerDisconnect(conn);
	}

	public override void OnStopServer()
	{
		OnServerStopped?.Invoke();

		RoomPlayers.Clear();
		GamePlayers.Clear();
	}

	public void NotifyPlayersOfReadyState() {
		foreach (var player in RoomPlayers) {
			player.HandleReadyToStart(IsReadyToStart());
		}
	}

	private bool IsReadyToStart() {
		if (numPlayers < minPlayers) { return false; }

		foreach (var player in RoomPlayers) {
			if (!player.IsReady) { return false; }
		}

		return true;
	}
	
	public void StartGame()
	{
		if (SceneManager.GetActiveScene().name == MenuScene.name) {
			if (!IsReadyToStart()) { return; }

			ServerChangeScene("Map2");
		}
	}

	public void NewRound() {
		ServerChangeScene(SceneManager.GetActiveScene().name);
	}

	public override void ServerChangeScene(string newSceneName)
	{
		previousSceneName = SceneManager.GetActiveScene().name;
		if (newSceneName.StartsWith("Map")) {
			PlayerSpawnSystem.ClearSpawnPoints();
			if (previousSceneName == "Menu") {
				for (int i = RoomPlayers.Count - 1; i >= 0; i--)
				{
					var conn = RoomPlayers[i].connectionToClient;
					var gameplayerInstance = Instantiate(gamePlayerPrefab);
					gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
					gameplayerInstance.SetTeam(RoomPlayers[i].Team);

					NetworkServer.Destroy(conn.identity.gameObject);

					NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
				}
			}
		}
		if (newSceneName == "GameEnd" && previousSceneName.StartsWith("Map"))
		{
			NetworkServer.Destroy(playerSpawnSystemInstance);
			playerSpawnSystemInstance = null;
			NetworkServer.Destroy(publicUIInstance);
			publicUIInstance = null;
		}
		if (newSceneName == "Menu" && roundSystemInstance != null) {
			NetworkServer.Destroy(roundSystemInstance);
			roundSystemInstance = null;
			roundSystemInstance2 = null;
		}

		base.ServerChangeScene(newSceneName);
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		if (previousSceneName == "Menu" && sceneName.StartsWith("Map"))
		{
			publicUIInstance = Instantiate(publicUI);
			NetworkServer.Spawn(publicUIInstance);
			playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
			NetworkServer.Spawn(playerSpawnSystemInstance);
			roundSystemInstance = Instantiate(roundSystem);
			roundSystemInstance2 = roundSystemInstance.GetComponent<RoundSystem>();
			NetworkServer.Spawn(roundSystemInstance);
			return;
		}
		if (sceneName.StartsWith("Map") && previousSceneName.StartsWith("Map")) {
			roundSystemInstance.GetComponent<RoundSystem>().StartFreeze();
		}
	}

	public override void OnServerReady(NetworkConnection conn)
	{
		base.OnServerReady(conn);

		OnServerReadied?.Invoke(conn);
	}

	[Server]
	public void SwitchTeams() {
		foreach (NetworkGamePlayerLobby player in GamePlayers) {
			player.IsGreen = !player.IsGreen;
			player.IsOrange = !player.IsOrange;
		}
	}
}
