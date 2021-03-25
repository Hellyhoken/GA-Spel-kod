using Mirror;
using UnityEngine;

public class NetworkGamePlayerLobby : NetworkBehaviour
{
	[SyncVar]
	public string displayName = "Loading...";
	[SyncVar]
	public bool IsOrange = false;
	[SyncVar]
	public bool IsGreen = false;
	[SyncVar]
	public int kills = 0;
	[SyncVar]
	public int deaths = 0;
	[SyncVar]
	public int money = 0;


	[SerializeField] private GameObject defaultPrimrary = null;
	[SerializeField] private GameObject defaultSecondary = null;

	public GameObject[] weapons = new GameObject[2];

	private NetworkManagerLobby room;

	public NetworkManagerLobby Room
	{
		get
		{
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerLobby;
		}
	}

	public override void OnStartClient()
	{
		DontDestroyOnLoad(gameObject);

		Room.GamePlayers.Add(this);
	}

	public override void OnNetworkDestroy()
	{
		Room.GamePlayers.Remove(this);
	}

	[Server]
	public void SetDisplayName(string displayName) {
		this.displayName = displayName;
	}

	[Server]
	public void SetTeam(string team)
	{
		if (team == "O") { this.IsOrange = true; }
		else if (team == "G") { this.IsGreen = true; }
	}

	[Command (ignoreAuthority = true)]
	public void CmdSetMoney(int value) {
		money = value;
	}

	[Server]
	public void SyncWeapons(WeaponSelect weaponSelect) {
		if (weaponSelect.weapons[0] != null) { weapons[0] = weaponSelect.weapons[0].GetComponent<Gun>().droped; }
		else { weapons[0] = defaultPrimrary; }
		if (weaponSelect.weapons[1] != null) { weapons[1] = weaponSelect.weapons[1].GetComponent<Gun>().droped; }
		else { weapons[1] = defaultSecondary; }
	}
}