using Mirror;
using UnityEngine;

public class PlayerCrouch : NetworkBehaviour
{
	[SerializeField] private CharacterController controller = null;
	[SerializeField] private Transform cameraHolder = null;
	[SerializeField] private PlayerMove playerMove = null;
	[SerializeField] private float crouchSpeedMultiplier = 4/7;
	private float playHeight = 0f;
	private float h = 0f;

	public override void OnStartAuthority()
	{
		enabled = true;

		playHeight = controller.height;
		h = playHeight;
		playerMove.crouchMultiplier = 1f;

		InputManager.Controls.Player.Crouch.performed += ctx => Crouch();
		InputManager.Controls.Player.Crouch.canceled += ctx => UnCrouch();
	}

	[ClientCallback]
	private void Update() => Crouching();

	[Client]
	private void Crouch()
	{
		h = playHeight * 0.5f;
	}

	[Client]
	private void UnCrouch()
	{
		h = playHeight;
	}

	private void Crouching()
	{
		float lastHeight = controller.height;
		controller.height = Mathf.Lerp(controller.height, h, 5 * Time.deltaTime);
		controller.center += new Vector3(0, (controller.height - lastHeight) / 2, 0);
		cameraHolder.position += new Vector3(0, controller.height - lastHeight, 0);
		playerMove.crouchMultiplier = Mathf.Lerp(crouchSpeedMultiplier, 1f, (controller.height - 0.5f * playHeight) / (0.5f * playHeight));
	}
}
