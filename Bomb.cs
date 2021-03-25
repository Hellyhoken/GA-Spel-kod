using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
	public float plantTime = 3.8f;
	public float drawTime = .5f;
	public Vector3 plantOffset;

	public GameObject plantedBomb;
	public LayerMask bombSite;
	public LayerMask groundMask;

	[SyncVar]
	bool isPlanting = false;
	[SyncVar]
	private float start = 0f;
	private bool wasPlanting = false;

	private WeaponSelect weaponSelector = null;

	private Coroutine planting;

	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> plant = null;
	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> stopPlant = null;

	private RoundSystem roundSystem;
	public RoundSystem RoundSystem
	{
		get
		{
			if (roundSystem != null) { return roundSystem; }
			return roundSystem = GameObject.Find("RoundSystem(Clone)").GetComponent<RoundSystem>();
		}
	}

	private UIPlantManager plantManager;
	public UIPlantManager PlantManager
	{
		get
		{
			if (plantManager != null) { return plantManager; }
			return plantManager = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UIPlantManager>();
		}
	}

	public override void OnStartAuthority()
	{
		enabled = true;
		plant += ctx => CmdCanPlant();
		stopPlant += ctx => CmdStopPlant();
	}

	private void OnEnable()
	{
		if (!hasAuthority) { return; }
		InputManager.Controls.Player.Fire1.performed += this.plant;
		InputManager.Controls.Player.Fire1.canceled += this.stopPlant;
		InputManager.Controls.Player.Use.performed += this.plant;
		InputManager.Controls.Player.Use.canceled += this.stopPlant;
	}

	private void OnDisable()
	{
		if (!hasAuthority) { return; }
		InputManager.Controls.Player.Fire1.performed -= this.plant;
		InputManager.Controls.Player.Fire1.canceled -= this.stopPlant;
		InputManager.Controls.Player.Use.performed -= this.plant;
		InputManager.Controls.Player.Use.canceled -= this.stopPlant;
	}

	private void OnDestroy()
	{
		if (!hasAuthority) { return; }
		InputManager.Controls.Player.Fire1.performed -= this.plant;
		InputManager.Controls.Player.Fire1.canceled -= this.stopPlant;
		InputManager.Controls.Player.Use.performed -= this.plant;
		InputManager.Controls.Player.Use.canceled -= this.stopPlant;
	}

	private void Update()
	{
		if (hasAuthority) {
			if (isPlanting)
			{
				PlantManager.Planting((float)NetworkTime.time - start / plantTime);
			}
			else if (wasPlanting && !isPlanting) {
				PlantManager.StopPlanting();
			}
			wasPlanting = isPlanting;
		}
	}

	[Command]
	private void CmdCanPlant() {
		if (isPlanting) { return; }
		if (Physics.CheckSphere(transform.root.position, 1f, bombSite)) {
			planting = StartCoroutine(bombPlanting());
			start = (float)NetworkTime.time;
		}
	}

	[Command]
	private void CmdStopPlant()
	{
		StopCoroutine(planting);
		isPlanting = false;
	}

	[Server]
	private void BombPlant() {
		RaycastHit hit;
		Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundMask);
		NetworkServer.Spawn(Instantiate(plantedBomb, hit.point + plantOffset, Quaternion.LookRotation(hit.normal)));
		weaponSelector.currentWeapon = weaponSelector.FindNextWeapon(-1, 1, true);
		weaponSelector.SelectWeapon(weaponSelector.currentWeapon);
		RoundSystem.PlantBomb();
		NetworkServer.Destroy(gameObject);
	}

	[Server]
	public void SetupBomb() {
		weaponSelector = transform.root.GetComponent<WeaponSelect>();
	}

	IEnumerator bombPlanting() {
		isPlanting = true;
		yield return new WaitForSeconds(plantTime);
		BombPlant();
	}
}