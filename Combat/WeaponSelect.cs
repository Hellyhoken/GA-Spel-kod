using Mirror;
using UnityEngine;

public class WeaponSelect : NetworkBehaviour
{
	[SerializeField] private WeaponDrop weaponDrop = null;
	[SerializeField] public Transform weaponHolder = null;
	
	[SerializeField] public GameObject[] weapons;

	public int currentWeapon = -1;

	private int previousWeapon = 0;

	public override void OnStartAuthority()
	{
		enabled = true;

		currentWeapon = FindNextWeapon(currentWeapon, 1, true);
		CmdSelectWeapon(currentWeapon);

		InputManager.Controls.Player.Primrary.performed += ctx => Primrary();
		InputManager.Controls.Player.Secondary.performed += ctx => Secondary();
		InputManager.Controls.Player.Melee.performed += ctx => Melee();
		InputManager.Controls.Player.Grenades.performed += ctx => Grenades();
		InputManager.Controls.Player.Bomb.performed += ctx => Bomb();
		InputManager.Controls.Player.Scroll.performed += ctx => Scroll(ctx.ReadValue<float>());
	}

	void Update()
	{
		if (!hasAuthority) { return; }
		if (weapons[currentWeapon] == null)
		{
			currentWeapon = FindNextWeapon(currentWeapon, 1, true);
		}

		previousWeapon = currentWeapon;

		CmdSyncToServer(weapons[0], weapons[1]);
	}

	[Command]
	public void CmdSelectWeapon(int curWeapon) {
		currentWeapon = curWeapon;
		RpcSelectWeapon(currentWeapon);
	}

	[ClientRpc]
	private void RpcSelectWeapon(int curWeapon)
	{
		currentWeapon = curWeapon;
		if (weapons[currentWeapon] != null)
		{
			foreach (GameObject i in weapons)
			{
				if (i != null)
				{
					i.SetActive(false);
				}
			}
			weapons[currentWeapon].SetActive(true);

		}
		else
		{
			currentWeapon = previousWeapon;
		}
	}

	public int FindNextWeapon(int curWeapon, int move, bool going)
	{
		if (curWeapon != 3 && going)
		{
			curWeapon += move;
		}
		if (curWeapon == 3)
		{
			bool go = weapons[3].GetComponent<GrenadeHolder>().CycleGrenades(move, false);
			if (go)
			{
				curWeapon += move;
				return FindNextWeapon(curWeapon, move, false);
			}
			else
			{
				return curWeapon;
			}
		}

		if (curWeapon < 0)
		{
			curWeapon = 4;
		}
		else if (curWeapon > 4)
		{
			curWeapon = 0;
		}

		if (weapons[curWeapon] == null)
		{
			return FindNextWeapon(curWeapon, move, true);
		}
		else
		{
			return curWeapon;
		}
	}

	private void Scroll(float v)
	{
		currentWeapon = FindNextWeapon(currentWeapon, (int)v, true);
		if (currentWeapon != previousWeapon)
		{
			CmdSelectWeapon(currentWeapon);
		}
	}

	private void Bomb()
	{
		currentWeapon = 4;
		if (currentWeapon != previousWeapon)
		{
			CmdSelectWeapon(currentWeapon);
		}
	}

	private void Grenades()
	{
		if (weapons[3].GetComponent<GrenadeSelect>().HasGrenade())
		{
			currentWeapon = 3;
			if (currentWeapon != previousWeapon)
			{
				CmdSelectWeapon(currentWeapon);
			}
			weapons[3].GetComponent<GrenadeSelect>().CycleGrenades(1, true);
		}
	}

	private void Melee()
	{
		currentWeapon = 2;
		if (currentWeapon != previousWeapon)
		{
			CmdSelectWeapon(currentWeapon);
		}
	}

	private void Secondary()
	{
		currentWeapon = 1;
		if (currentWeapon != previousWeapon)
		{
			CmdSelectWeapon(currentWeapon);
		}
	}

	private void Primrary()
	{
		currentWeapon = 0;
		if (currentWeapon != previousWeapon)
		{
			CmdSelectWeapon(currentWeapon);
		}
	}

	public void SelectWeapon(int curWeapon) {
		if (!hasAuthority) { return; }
		CmdSelectWeapon(curWeapon);
	}

	public void RemoveWeapons() {
		if (weapons[0] != null) {
			currentWeapon = 0;
			weaponDrop.Drop();
			weapons[1] = null;
		}
		else if (weapons[1] != null) {
			currentWeapon = 1;
			weaponDrop.Drop();
		}
		currentWeapon = 2;
		SelectWeapon(currentWeapon);
		weapons[3].GetComponent<GrenadeSelect>().grenades = new GameObject[4];

		CmdSync();
	}

	[Command]
	private void CmdSync() {
		PlayerID playerID = transform.GetComponent<PlayerID>();
		playerID.Room.GamePlayers[playerID.gamePlayerIndex].SyncWeapons(this);
	}

	[Server]
	private void Sync() {
		PlayerID playerID = transform.GetComponent<PlayerID>();
		playerID.Room.GamePlayers[playerID.gamePlayerIndex].SyncWeapons(this);
	}

	[Command (ignoreAuthority = true)]
	public void CmdSyncToServer(GameObject prim, GameObject sec) {
		weapons[0] = prim;
		weapons[1] = sec;
		Sync();
	}
}
