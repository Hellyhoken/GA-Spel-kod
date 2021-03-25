using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
{
	[SerializeField] private GameObject playerPrefab = null;
	[SerializeField] private GameObject orangeCharacterPrefab = null;
	[SerializeField] private GameObject greenCharacterPrefab = null;

	private static List<Transform> orangeSpawnPoints = new List<Transform>();
	private static List<Transform> greenSpawnPoints = new List<Transform>();

	private static int orangeNextIndex = 0;
	private static int greenNextIndex = 0;
	
	public static List<GameObject> players = new List<GameObject>();
	private static List<GameObject> characters = new List<GameObject>();

	private RoundSystem roundSystem;
	public RoundSystem RoundSystem
	{
		get
		{
			if (roundSystem != null) { return roundSystem; }
			return roundSystem = GameObject.Find("RoundSystem(Clone)").GetComponent<RoundSystem>();
		}
	}

	private void Awake() => DontDestroyOnLoad(this.gameObject);

	public static void AddSpawnPoint(Transform transform, string team) {
		if (string.IsNullOrEmpty(team)) { return; }
		if (team == "O") { orangeSpawnPoints.Add(transform); }
		else if (team == "G") { greenSpawnPoints.Add(transform); }
	}

	public static void ClearSpawnPoints() {
		orangeSpawnPoints.Clear();
		greenSpawnPoints.Clear();
		orangeNextIndex = 0;
		greenNextIndex = 0;
		players.Clear();
		characters.Clear();
	}

	public override void OnStartServer() => NetworkManagerLobby.OnServerReadied += SpawnPlayer;

	[ServerCallback]
	private void OnDestroy() => NetworkManagerLobby.OnServerReadied -= SpawnPlayer;

	[Server]
	public void SpawnPlayer(NetworkConnection conn)
	{

		NetworkGamePlayerLobby networkGamePlayerLobby = conn.identity.transform.GetComponent<NetworkGamePlayerLobby>();

		if (networkGamePlayerLobby.IsOrange) {
			Transform spawnpoint = orangeSpawnPoints[orangeNextIndex];

			if (spawnpoint == null) {
				Debug.LogError($"Missing spawn point for orange {orangeNextIndex}");
				return;
			}
			GameObject playerInstance = Instantiate(playerPrefab, orangeSpawnPoints[orangeNextIndex].position, orangeSpawnPoints[orangeNextIndex].rotation);
			PlayerID playerID = playerInstance.GetComponent<PlayerID>();
			playerID.isOrange = true;
			playerID.displayName = networkGamePlayerLobby.displayName;
			playerID.SetGamePlayer(RoundSystem.Room.GamePlayers.IndexOf(networkGamePlayerLobby));
			playerInstance.GetComponent<MoneyHandler>().SetMoney(networkGamePlayerLobby.money, networkGamePlayerLobby);
			players.Add(playerInstance);
			NetworkServer.Spawn(playerInstance, conn);
			GameObject characterInstance = Instantiate(orangeCharacterPrefab, playerInstance.transform);
			characters.Add(characterInstance);
			NetworkServer.Spawn(characterInstance, conn);

			GiveWeapons(playerInstance, networkGamePlayerLobby.gameObject);

			GameObject.Find("OrangeDeadRoom").GetComponent<SpectatorScreenController>().AddPlayer(playerInstance);

			for (int i = 0; i < players.Count; i++)
			{
				RpcSetCharacterParent(characters[i], players[i]);
			}

			orangeNextIndex++;
		}
		else if (networkGamePlayerLobby.IsGreen)
		{
			Transform spawnpoint = greenSpawnPoints[greenNextIndex];

			if (spawnpoint == null)
			{
				Debug.LogError($"Missing spawn point for green {greenNextIndex}");
				return;
			}
			GameObject playerInstance = Instantiate(playerPrefab, greenSpawnPoints[greenNextIndex].position, greenSpawnPoints[greenNextIndex].rotation);
			PlayerID playerID = playerInstance.GetComponent<PlayerID>();
			playerID.isGreen = true;
			playerID.displayName = networkGamePlayerLobby.displayName;
			playerID.SetGamePlayer(RoundSystem.Room.GamePlayers.IndexOf(networkGamePlayerLobby));
			playerInstance.GetComponent<MoneyHandler>().SetMoney(networkGamePlayerLobby.money, networkGamePlayerLobby);
			players.Add(playerInstance);
			NetworkServer.Spawn(playerInstance, conn);
			GameObject characterInstance = Instantiate(greenCharacterPrefab, playerInstance.transform);
			characters.Add(characterInstance);
			NetworkServer.Spawn(characterInstance, conn);

			GiveWeapons(playerInstance, networkGamePlayerLobby.gameObject);

			GameObject.Find("GreenDeadRoom").GetComponent<SpectatorScreenController>().AddPlayer(playerInstance);

			for (int i = 0; i < players.Count; i++) {
				RpcSetCharacterParent(characters[i], players[i]);
			}

			greenNextIndex++;
		}
	}

	[Server]
	public void SyncMoney() {
		foreach (GameObject player in players) {
			MoneyHandler moneyHandler = player.GetComponent<MoneyHandler>();
			moneyHandler.SetMoney(moneyHandler.player.money, moneyHandler.player);
		}
	}

	[ClientRpc]
	private void RpcSetCharacterParent(GameObject child, GameObject parent) {
		Transform ct = child.transform;
		ct.SetParent(parent.transform);
		ct.localPosition = new Vector3(0,-0.1f,0);
		ct.localRotation = Quaternion.Euler(Vector3.zero);
	}

	[Server]
	private void GiveWeapons(GameObject player, GameObject gamePlayer) {
		NetworkGamePlayerLobby playerLobby = gamePlayer.GetComponent<NetworkGamePlayerLobby>();
		foreach (GameObject weapon in playerLobby.weapons) {
			if (weapon == null) { continue; }
			GameObject newWeapon = Instantiate(weapon, player.transform.position + new Vector3(0,3,0), player.transform.rotation);
			NetworkServer.Spawn(newWeapon);
		}
	}
}
