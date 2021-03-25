using System;
using Fragsurf.Movement;
using Mirror;
using UnityEngine;

public class Grenade : NetworkBehaviour
{
	[SerializeField] private Vector3 throwForce;
	[SerializeField] private Transform throwingPoint = null;
	[SerializeField] private SurfCharacter playerMovement = null;
	[SerializeField] private WeaponSelect weaponSelector = null;
	[SerializeField] private GameObject thrownGrenade = null;
	private Vector3 force;

	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> throwGrenade = null;

	private bool throwing = false;

	public override void OnStartAuthority()
	{
		throwGrenade += ctx => CmdThrowGrenade();
	}

	private void OnEnable()
	{
		if (!hasAuthority) { return; }
		InputManager.Controls.Player.Fire1.canceled += this.throwGrenade;
	}

	private void OnDisable()
	{
		if (!hasAuthority) { return; }
		InputManager.Controls.Player.Fire1.canceled -= this.throwGrenade;
	}

	private void OnDestroy()
	{
		if (!hasAuthority) { return; }
		InputManager.Controls.Player.Fire1.canceled -= this.throwGrenade;
	}

	[Command]
	private void CmdThrowGrenade() {
		if (throwing) { return; }
		throwing = true;
		GameObject grenade = Instantiate(thrownGrenade, throwingPoint.position , throwingPoint.rotation);
		NetworkServer.Spawn(grenade, netIdentity.connectionToClient);
		force = throwForce + playerMovement.moveData.velocity;
		grenade.GetComponent<GrenadeThrown>().GrenadeTimer(force, transform.root.gameObject);
		weaponSelector.currentWeapon = weaponSelector.FindNextWeapon(-1, 1, true);
		weaponSelector.SelectWeapon(weaponSelector.currentWeapon);
		NetworkServer.Destroy(gameObject);
	}

	[Command]
	public void CmdSetupGrenade() {
		playerMovement = transform.root.GetComponent<SurfCharacter>();
		throwingPoint = playerMovement.gameObject.FindComponentInChildWithTag<Transform>("Throwing Point");
		weaponSelector = playerMovement.transform.GetComponentInChildren<WeaponSelect>();
	}
}
