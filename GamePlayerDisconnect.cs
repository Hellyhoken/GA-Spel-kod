using Mirror;
using UnityEngine;

public class GamePlayerDisconnect : NetworkBehaviour
{
	[Command]
	public void CmdDisconnect()
	{
		this.netIdentity.connectionToClient.Disconnect();
	}
}
