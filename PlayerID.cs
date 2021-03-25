using UnityEngine;
using Mirror;

public class PlayerID : NetworkBehaviour
{
	[SyncVar]
	public string displayName;

	[SyncVar]
	public bool isOrange = false;
	[SyncVar]
	public bool isGreen = false;

	[SyncVar]
	public int gamePlayerIndex = 0;

	private NetworkManagerLobby room;
	public NetworkManagerLobby Room
	{
		get
		{
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerLobby;
		}
	}
	
	[Command(ignoreAuthority = true)]
	public void CmdAddKill() {
		Room.GamePlayers[gamePlayerIndex].kills++;
	}

	[Command]
	public void CmdAddDeath()
	{
		Room.GamePlayers[gamePlayerIndex].deaths++;
	}

	public void SetGamePlayer(int player) {
		gamePlayerIndex = player;
	}
}
