using Mirror;
using UnityEngine;

public class PlayerJump : NetworkBehaviour
{
	[SerializeField] public float jumpHeight = 2f;
	[SerializeField] private PlayerGravity playerGravity = null;

	private Vector2 previousInput;

	public override void OnStartAuthority()
	{
		enabled = true;

		InputManager.Controls.Player.Jump.performed += ctx => Jump();
	}

	[Client]
	private void Jump() {
		if (playerGravity.isGrounded) { playerGravity.velocity.y = Mathf.Sqrt(-2 * jumpHeight * playerGravity.gravity); }
	}
}
