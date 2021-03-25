using UnityEngine;
using Mirror;

public class WeaponDrop : NetworkBehaviour
{
	[SerializeField] private WeaponSelect weaponSelector;
	[SerializeField] private Transform throwingPoint;
	[SerializeField] private Vector3 force;
	[SerializeField] private GameObject player;

	public override void OnStartAuthority()
	{
		InputManager.Controls.Player.Drop.performed += ctx => Drop();
	}

	public void Drop()
	{
		CmdDrop();
	}

	[Command]
	private void CmdDrop() {
		Gun gun = weaponSelector.weapons[weaponSelector.currentWeapon].GetComponent<Gun>();
		if (gun == null) { return; }
		GameObject drop = Instantiate(gun.droped, weaponSelector.gameObject.FindComponentInChildWithTag<Transform>("Throwing Point").position, weaponSelector.transform.rotation);
		WeaponPickup wPickup = drop.GetComponent<WeaponPickup>();
		wPickup.skipPlayer = weaponSelector.gameObject;
		wPickup.SkipPlayer();
		NetworkServer.Spawn(drop);
		drop.GetComponent<Rigidbody>().AddRelativeForce(force, ForceMode.VelocityChange);
		Debug.Log(weaponSelector.weapons[weaponSelector.currentWeapon]);
		weaponSelector.weapons[weaponSelector.currentWeapon] = null;
		RpcDrop();
		NetworkServer.Destroy(gun.gameObject);
		PlayerID playerID = weaponSelector.transform.GetComponent<PlayerID>();
		playerID.Room.GamePlayers[playerID.gamePlayerIndex].SyncWeapons(weaponSelector);
	}

	[ClientRpc]
	private void RpcDrop() {
		if (isClientOnly) { weaponSelector.weapons[weaponSelector.currentWeapon] = null; }
		if (!weaponSelector.hasAuthority) { return; }
		int curWeapon = weaponSelector.FindNextWeapon(-1, 1, true);
		weaponSelector.SelectWeapon(curWeapon);
		weaponSelector.currentWeapon = curWeapon;
	}
}
