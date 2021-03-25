using Mirror;
using System.Collections;
using UnityEngine;

public class WeaponPickup : NetworkBehaviour
{
	[SerializeField] private int slot = 0;
	[SerializeField] private bool isGrenade;
	[SerializeField] private bool isBomb;
	[SerializeField] public int ammo = 0;
	[SerializeField] public int reserve = 0;

	[SerializeField] private GameObject camWeapon;

	public GameObject skipPlayer;
	public bool skip;
	private bool isTaken = false;

	[ServerCallback]
	private void OnTriggerEnter(Collider collision)
	{
		if (isTaken) { return; }
		if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerPickup"))
		{
			if (skip && skipPlayer == collision.transform.root.gameObject) { return; }
			isTaken = true;
			WeaponSelect weaponSelector = collision.transform.root.GetComponent<WeaponSelect>();
			PlayerID playerID = collision.transform.root.GetComponent<PlayerID>();
			if (isBomb && playerID.isOrange)
			{
				PickupW(weaponSelector.gameObject);
			}
			else if (isGrenade)
			{
				PickupG(weaponSelector.gameObject);
			}
			else if (!isBomb && weaponSelector.weapons[slot] == null)
			{
				PickupW(weaponSelector.gameObject);
			}
		}
	}
	
	[Server]
	private void PickupW(GameObject player)
	{
		WeaponSelect weaponSelector = player.transform.GetComponent<WeaponSelect>();
		GameObject newWeapon = Instantiate(camWeapon, weaponSelector.weaponHolder.position, Quaternion.LookRotation(-weaponSelector.weaponHolder.transform.forward, weaponSelector.weaponHolder.transform.up));
		NetworkServer.Spawn(newWeapon, weaponSelector.connectionToClient);
		newWeapon.transform.SetParent(weaponSelector.weaponHolder);
		RpcPickupW(player, newWeapon);
		PlayerID playerID = weaponSelector.transform.GetComponent<PlayerID>();
		playerID.Room.GamePlayers[playerID.gamePlayerIndex].SyncWeapons(weaponSelector);
	}

	[ClientRpc]
	private void RpcPickupW(GameObject player, GameObject newWeapon)
	{
		WeaponSelect weaponSelector = player.transform.GetComponent<WeaponSelect>();

		newWeapon.transform.SetParent(weaponSelector.weaponHolder);
		newWeapon.transform.localPosition = newWeapon.transform.GetComponent<CamWeapon>().offset;
		newWeapon.transform.rotation = Quaternion.LookRotation(-weaponSelector.weaponHolder.transform.forward, weaponSelector.weaponHolder.transform.up);
		if (weaponSelector.weapons[slot] == null)
		{
			weaponSelector.weapons[slot] = newWeapon;

			Gun gun = newWeapon.transform.GetComponent<Gun>();
			if (gun != null)
			{
				if (gun.hasAuthority) { gun.CmdSetupGun(setAmmo: ammo, setReserve: reserve); }
				if (weaponSelector.hasAuthority) { weaponSelector.CmdSyncToServer(weaponSelector.weapons[0], weaponSelector.weapons[1]); }
				newWeapon.SetActive(false);
				if (weaponSelector.hasAuthority) { CmdNo(gameObject); }
				return;
			}
			Bomb bomb = newWeapon.transform.GetComponent<Bomb>();
			if (bomb != null)
			{
				bomb.SetupBomb();
				newWeapon.SetActive(false);
				if (weaponSelector.hasAuthority) { CmdNo(gameObject); }
				return;
			}
		}
		if (weaponSelector.hasAuthority) { CmdNo(newWeapon); }
	}
	
	[Server]
	private void PickupG(GameObject player) {
		WeaponSelect weaponSelector = player.transform.GetComponent<WeaponSelect>();
		GameObject newWeapon = Instantiate(camWeapon, weaponSelector.weapons[3].transform);
		NetworkServer.Spawn(newWeapon, weaponSelector.connectionToClient);
		RpcPickupG(player, newWeapon, newWeapon.transform.localPosition, newWeapon.transform.localRotation);
		PlayerID playerID = weaponSelector.transform.GetComponent<PlayerID>();
		playerID.Room.GamePlayers[playerID.gamePlayerIndex].SyncWeapons(weaponSelector);
	}

	[ClientRpc]
	private void RpcPickupG(GameObject player, GameObject newWeapon, Vector3 pos, Quaternion rot)
	{
		WeaponSelect weaponSelector = player.transform.GetComponent<WeaponSelect>();

		newWeapon.transform.SetParent(weaponSelector.weapons[3].transform);
		newWeapon.transform.localPosition = pos;
		newWeapon.transform.localRotation = rot;

		GrenadeSelect grenadeHolder = weaponSelector.weapons[3].transform.GetComponent<GrenadeSelect>();
		if (grenadeHolder.grenades[slot] == null)
		{
			grenadeHolder.grenades[slot] = newWeapon;
			Grenade grenade = newWeapon.transform.GetComponent<Grenade>();
			if (grenade != null)
			{
				if (grenade.hasAuthority) { grenade.CmdSetupGrenade(); }
			}
			newWeapon.SetActive(false);
			if (weaponSelector.hasAuthority) { CmdNo(gameObject); }
			return;
		}
		else if (slot == 0 && grenadeHolder.grenades[1] == null)
		{
			grenadeHolder.grenades[1] = newWeapon;
			Grenade grenade = newWeapon.transform.GetComponent<Grenade>();
			if (grenade != null)
			{
				if (grenade.hasAuthority) { grenade.CmdSetupGrenade(); }
			}
			newWeapon.SetActive(false);
			if (weaponSelector.hasAuthority) { CmdNo(gameObject); }
			return;
		}
		if (weaponSelector.hasAuthority) { CmdNo(newWeapon); }
	}

	[Command (ignoreAuthority = true)]
	private void CmdNo(GameObject destroy) {
		if (destroy != gameObject) { isTaken = false; }
		NetworkServer.Destroy(destroy);
	}

	public void SkipPlayer()
	{
		StartCoroutine(SkipPlay());
	}

	IEnumerator SkipPlay()
	{
		skip = true;
		yield return new WaitForSeconds(2f);
		skip = false;
	}
}
