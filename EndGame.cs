using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : NetworkBehaviour
{
	[SerializeField] private GameObject winText = null;

	private NetworkManagerLobby room;
	public NetworkManagerLobby Room
	{
		get
		{
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerLobby;
		}
	}

	[ServerCallback]
	private void Awake()
	{
		NetworkManagerLobby.OnServerStopped += CleanUpServer;
		NetworkManagerLobby.OnServerReadied += CheckToStartRound;
	}

	[ServerCallback]
	private void OnDestroy() => CleanUpServer();

	[Server]
	private void CleanUpServer()
	{
		NetworkManagerLobby.OnServerStopped -= CleanUpServer;
		NetworkManagerLobby.OnServerReadied -= CheckToStartRound;
	}

	[Server]
	private void CheckToStartRound(NetworkConnection conn)
	{
		if (SceneManager.GetActiveScene().name != "GameEnd") { return; }
		
		if (Room.GamePlayers.Count(x => x.connectionToClient.isReady) != Room.GamePlayers.Count) { return; }

		SpawnText();
	}

	[ServerCallback]
	private void SpawnText() {
		GameObject winTextInstance = Instantiate(winText, transform.position, transform.rotation);
		winTextInstance.transform.SetParent(transform);
		NetworkServer.Spawn(winTextInstance);
		RpcSetParent(winTextInstance.name);
	}

	[ClientRpc]
	private void RpcSetParent(string childName) {
		Transform child = GameObject.Find(childName).transform;
		child.SetParent(transform);
		child.localPosition = new Vector3(0,0,0);
		child.localScale = new Vector3(1,1,1);
	}
}
