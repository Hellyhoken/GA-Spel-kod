using Cinemachine;
using Mirror;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
	[SerializeField] private Transform playerBody = null;
	[SerializeField] private CinemachineVirtualCamera virtualCamera = null;
	[SerializeField] private Transform cameraHolder = null;
	[SerializeField] public float mouseSensitivity = 100f;
	[SerializeField] private SettingsManager settingsManager = null;
	[SerializeField] private float sensitivityCalibration = 0.05f;

	private float xRot = 0f;

	public override void OnStartAuthority()
	{
		virtualCamera.gameObject.SetActive(true);

		enabled = true;

		settingsManager.SetupSens();

		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update() => Look(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));

	private void Look(Vector2 lookAxis) {
		if (!InputManager.Controls.Player.Look.enabled) { return; }

		playerBody.Rotate(0f, lookAxis.x * mouseSensitivity * sensitivityCalibration, 0f);

		xRot = Mathf.Clamp(xRot + -lookAxis.y * mouseSensitivity * sensitivityCalibration, -90f, 90f);
		cameraHolder.localRotation = Quaternion.Euler(xRot, 0f, 0f);

		CmdLook(xRot);
	}

	[Command]
	private void CmdLook(float xRot)
	{
		this.xRot = xRot;

		cameraHolder.localRotation = Quaternion.Euler(xRot, 0f, 0f);

		RpcLook(xRot);
	}

	[ClientRpc]
	private void RpcLook(float xRot) {
		if (hasAuthority) { return; }

		this.xRot = xRot;

		cameraHolder.localRotation = Quaternion.Euler(xRot, 0f, 0f);
	}
}
