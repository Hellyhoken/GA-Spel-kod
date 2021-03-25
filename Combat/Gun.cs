using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Fragsurf.Movement;

public class Gun : NetworkBehaviour
{
	[SerializeField] public float damage = 10f;
	[SerializeField] private float fireRate = 10f;
	[SerializeField] public int maxAmmo = 30;
	[SerializeField] public int maxReserve = 30;
	[SerializeField] private float reloadTime = 1f;
	[SerializeField] public float recoilResetTime = 0.5f;
	[SerializeField] public float spreadResetTime = 0.7f;
	[SerializeField] public float recoilSpread = 0.5f;
	[SerializeField] public float movingSpread = 4f;
	[SerializeField] public float jumpingSpread = 6f;
	[SerializeField] public float noScopeSpread = 15f;
	[SerializeField] public float hitForce = 20f;
	[SerializeField] private int iconIndex = 0;

	public Transform shootingPoint = null;
	[SerializeField] public GameObject muzzleFlash = null;
	[SerializeField] public GameObject hitFlash = null;
	public RecoilController recoilCont = null;
	[SerializeField] public GameObject gunSound = null;
	[SerializeField] public RecoilPattern pattern = null;
	public SurfCharacter playMove = null;
	[SerializeField] public GameObject droped = null;
	private UITimeManager roundManager = null;
	private Animator animator = null;
	private GameObject scopeOverlay = null;
	private Transform cameraHolder = null;
	private CinemachineVirtualCamera mainCamera = null;
	private PlayerCameraController mouseLook = null;
	private GameObject crosshair = null;
	[SerializeField] private AmmoDisplay ammoDisplay = null;
 
	[SerializeField] public float scopedFOV = 15f;
	private float prevFOV;

	private float nextShot = 0;
	public int ammo = 0;
	public int reserve = 0;
	private int shot = 0;
	private bool isReloading = false;
	private bool isSetup = false;
	[SerializeField] private bool semiAuto = false;
	[SerializeField] public bool sniperRifle = false;
	private bool shouldShootBool = false;
	public bool isScoping = false;
	public float spreadMaxDev = 0f;

	[SerializeField] private LayerMask ignoreShotMask;

	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> setShot = null;
	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> startSemiShoot = null;
	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> startAutoShoot = null;
	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> stopShoot = null;
	private Action<UnityEngine.InputSystem.InputAction.CallbackContext> scope = null;

	public override void OnStartAuthority()
	{
		setShot += ctx => SetShot();
		scope += ctx => Scope();
		startSemiShoot += ctx => SemiAutoShoot();
		startAutoShoot += ctx => AutoShoot();
		stopShoot += ctx => AutoRelease();
	}

	private void OnEnable() {
		if (!hasAuthority) { return; }
		if (!isSetup) { return; }
		ammoDisplay.UpdateDisplay(ammo, reserve);
		isReloading = false;
		InputManager.Controls.Player.Fire1.performed += this.setShot;
		if (sniperRifle)
		{
			InputManager.Controls.Player.Fire2.performed += this.scope;
		}
		if (semiAuto) {
			InputManager.Controls.Player.Fire1.performed += this.startSemiShoot;
			return;
		}
		InputManager.Controls.Player.Fire1.performed += this.startAutoShoot;
		InputManager.Controls.Player.Fire1.canceled += this.stopShoot;
	}

	private void OnDisable()
	{
		if (!hasAuthority) { return; }
		if (!isSetup) { return; }
		ammoDisplay.UnSetDisplay();
		if (isScoping) {
			Scope();
		}
		shouldShootBool = false;
		InputManager.Controls.Player.Fire1.performed -= setShot;
		if (sniperRifle)
		{
			InputManager.Controls.Player.Fire2.performed -= this.scope;
		}
		if (semiAuto)
		{
			InputManager.Controls.Player.Fire1.performed -= this.startSemiShoot;
			return;
		}
		InputManager.Controls.Player.Fire1.performed -= this.startAutoShoot;
		InputManager.Controls.Player.Fire1.canceled -= this.stopShoot;
	}

	private void OnDestroy()
	{
		if (!hasAuthority) { return; }
		if (!isSetup) { return; }
		ammoDisplay.UnSetDisplay();
		if (isScoping)
		{
			Scope();
		}
		shouldShootBool = false;
		InputManager.Controls.Player.Fire1.performed -= this.setShot;
		if (sniperRifle)
		{
			InputManager.Controls.Player.Fire2.performed -= this.scope;
		}
		if (semiAuto)
		{
			InputManager.Controls.Player.Fire1.performed -= this.startSemiShoot;
			return;
		}
		InputManager.Controls.Player.Fire1.performed -= this.startAutoShoot;
		InputManager.Controls.Player.Fire1.canceled -= this.stopShoot;
	}

	void Update() {
		if (!hasAuthority) { return; }

		if (!isSetup) { return; }

		if (!roundManager.isRound) { return; }
		
		float time = Time.time;
		spreadMaxDev = 0f;
		if (isReloading)
		{
			return;
		}
		if (ammo <= 0 || InputManager.Controls.Player.Reload.triggered && ammo != maxAmmo)
		{
			isReloading = true;
			CmdReload();
			StartCoroutine(Reload());
			return;
		}
		if (shouldShootBool && time >= nextShot)
		{
			if (playMove.groundObject == null)
			{
				spreadMaxDev += jumpingSpread;
			}
			float speedPercent = playMove.moveData.velocity.magnitude / playMove.moveConfig.maxSpeed;
			spreadMaxDev += speedPercent * movingSpread;
			if (time < nextShot + spreadResetTime)
			{
				spreadMaxDev += ((time - nextShot) / spreadResetTime) * recoilSpread;
			}
			CmdShoot(spreadMaxDev, shot);
			ammo -= 1;
			nextShot = time + 1 / fireRate;
			shot++;
			ammoDisplay.UpdateDisplay(ammo, reserve);
		}
	}

	private void Scope()
	{
		isScoping = !isScoping;
		animator.SetBool("isScoped", isScoping);

		if (isScoping)
			StartCoroutine(OnScoped());
		else
			OnUnscoped();
	}

	private void AutoRelease()
	{
		shouldShootBool = false;
	}

	private void AutoShoot()
	{
		if (!roundManager.isRound) { return; }
		shouldShootBool = true;
	}

	private void SemiAutoShoot()
	{
		if (!roundManager.isRound || Time.time < nextShot || ammo <= 0) { return; }

		float time = Time.time;
		if (playMove.groundObject == null) { spreadMaxDev += jumpingSpread; }
		float speedPercent = Vector3.Scale(playMove.moveData.velocity, new Vector3(1, 0, 1)).magnitude / playMove.moveConfig.maxSpeed;
		spreadMaxDev += speedPercent * movingSpread;
		if (time < nextShot + spreadResetTime) { spreadMaxDev += ((time - nextShot) / spreadResetTime) * recoilSpread; }
		if (sniperRifle && !isScoping) { spreadMaxDev += noScopeSpread; }
		CmdShoot(spreadMaxDev, shot);
		ammo -= 1;
		nextShot = time + 1 / fireRate;
		shot++;
		ammoDisplay.UpdateDisplay(ammo, reserve);
	}

	private void OnUnscoped ()
	{
		scopeOverlay.SetActive(false);
		crosshair.SetActive(true);

		transform.GetComponent<MeshRenderer>().enabled = true;
		foreach (MeshRenderer mesh in transform.GetComponentsInChildren<MeshRenderer>())
		{
			mesh.enabled = true;
		}
		mainCamera.m_Lens.FieldOfView = prevFOV;
		mouseLook.mouseSensitivity = mouseLook.mouseSensitivity / (scopedFOV / prevFOV);

	}

	private IEnumerator OnScoped ()
	{
		yield return new WaitForSeconds(.1f);

		scopeOverlay.SetActive(true);
		crosshair.SetActive(false);

		transform.GetComponent<MeshRenderer>().enabled = false;
		foreach (MeshRenderer mesh in transform.GetComponentsInChildren<MeshRenderer>()) {
			mesh.enabled = false;
		}
		prevFOV = mainCamera.m_Lens.FieldOfView;
		mainCamera.m_Lens.FieldOfView = scopedFOV;
		mouseLook.mouseSensitivity = mouseLook.mouseSensitivity * (scopedFOV / prevFOV);

	}

	private IEnumerator Reload() {
		yield return new WaitForSeconds(reloadTime);
		isReloading = false;
		int newRes = Mathf.Max(reserve - maxAmmo + ammo, 0);
		ammo += reserve - newRes;
		reserve = newRes;
		ammoDisplay.UpdateDisplay(ammo, reserve);
	}

	[Command]
	public void CmdSetupGun(int setAmmo, int setReserve) {
		playMove = transform.GetComponentInParent<SurfCharacter>();
		recoilCont = playMove.transform.GetComponentInChildren<RecoilController>();
		cameraHolder = playMove.gameObject.FindComponentInChildWithTag<Transform>("CameraHolder");
		shootingPoint = recoilCont.transform;
		roundManager = GameObject.Find("Canvas_PublicUI(Clone)").transform.GetComponent<UITimeManager>();
		ammo = setAmmo;
		reserve = setReserve;
		RpcSetupGun(ammo, reserve);
	}

	[ClientRpc]
	public void RpcSetupGun(int setAmmo, int setReserve)
	{
		if (!hasAuthority) { return; }
		playMove = gameObject.GetComponentInParent<SurfCharacter>(true);
		ammoDisplay = playMove.transform.GetComponent<AmmoDisplay>();
		roundManager = GameObject.Find("Canvas_PublicUI(Clone)").transform.GetComponent<UITimeManager>();
		mainCamera = playMove.transform.GetComponentInChildren<CinemachineVirtualCamera>();
		ammo = setAmmo;
		reserve = setReserve;
		scopeOverlay = playMove.gameObject.FindComponentInChildWithTag<Image>("Scope").gameObject;
		mouseLook = playMove.transform.GetComponent<PlayerCameraController>();
		crosshair = playMove.gameObject.FindComponentInChildWithTag<Image>("Crosshair").gameObject;
		animator = transform.GetComponent<Animator>();
		isSetup = true;
	}

	private void SetShot() {
		float time = Time.time;
		bool nr1 = false;
		bool nr2 = false;
		if (time < nextShot + recoilResetTime && time > nextShot)
		{
			shot = 1;
			nr1 = true;
		}
		else if (time < nextShot)
		{
			return;
		}
		else
		{
			shot = 0;
			nr2 = true;
		}
		CmdSetShot(time, nextShot, nr1, nr2);
	}

	[Command]
	private void CmdSetShot(float time, float nextShot, bool nr1, bool nr2) {
		if (nr1) {
			recoilCont.halfResetRecoil(1 - ((time - nextShot) / recoilResetTime));
		}
		if (nr2) {
			recoilCont.resetRecoilRot();
		}
	}

	[Command]
	private void CmdShoot(float spreadMax, int shot)
	{
		Vector3 forwardVector = Vector3.forward;
		float deviation = UnityEngine.Random.Range(0f, spreadMax);
		float angle = UnityEngine.Random.Range(0f, 360f);
		forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
		forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;
		forwardVector = shootingPoint.rotation * forwardVector;
		RaycastHit hit;
		if (Physics.Raycast(shootingPoint.position, forwardVector, out hit, Mathf.Infinity, ~ignoreShotMask))
		{
			Debug.Log(hit.collider.name);
			if (hit.collider.transform.gameObject.layer == LayerMask.NameToLayer("PlayerHitBox"))
			{
				Debug.Log(hit.transform.name);
				BodyPart bodyPart = hit.collider.transform.GetComponent<BodyPart>();
				PlayerHealthController player = hit.transform.root.GetComponent<PlayerHealthController>();
				if (player != null)
				{
					player.RpcTakeDamage(damage * bodyPart.damageMultiplier, playMove.gameObject, iconIndex);
				}
			}

			if (hit.transform.root.GetComponent<Rigidbody>() != null && hit.transform.tag != "Grenade")
			{
				hit.rigidbody.AddForce(forwardVector * hitForce);
			}

			RpcShoot(transform, cameraHolder, hit.point, Quaternion.LookRotation(hit.normal));
		}
		recoilCont.setRecoilRot(pattern.pattern[shot].y, pattern.pattern[shot].x);
	}

	[ClientRpc]
	private void RpcShoot(Transform gunTransform, Transform fpsCamera, Vector3 hitPos, Quaternion hitRot)
	{
		gunTransform.GetComponentInChildren<ParticleSystem>().Play();
		GameObject newSound = Instantiate(gunSound, gunTransform.position, gunTransform.rotation);
		newSound.transform.SetParent(fpsCamera);
		newSound.transform.GetComponent<AudioSource>().Play();
		Destroy(newSound, 2f);
		Destroy(Instantiate(hitFlash, hitPos, hitRot), 2f);
	}

	[Command]
	private void CmdReload()
	{
		shot = 0;
		recoilCont.resetRecoilRot();
	}
}
