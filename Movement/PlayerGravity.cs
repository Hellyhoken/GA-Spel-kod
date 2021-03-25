using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : NetworkBehaviour
{
	[SerializeField] public float gravity = -19.64f;
	[SerializeField] private Transform groundCheck = null;
	[SerializeField] private float groundDistance = 0.4f;
	[SerializeField] private LayerMask notGroundMask = 0;
	[SerializeField] private CharacterController controller = null;

	public bool isGrounded = false;
	public Vector3 velocity;

	public override void OnStartAuthority()
	{
		enabled = true;
	}

	[ClientCallback]
	private void Update() => Gravity();

	[Client]
	private void Gravity() {
		float deltaTime = Time.deltaTime;

		isGrounded = IsGrounded();

		if (isGrounded && velocity.y < 0f)
		{
			velocity.y = -2f;
		}

		velocity.y += gravity * deltaTime;

		controller.Move(velocity * deltaTime);
	}

	private bool IsGrounded() {
		foreach (Collider i in Physics.OverlapSphere(groundCheck.position, groundDistance, ~notGroundMask)) {
			if (IsNotSelf(transform, i.transform)) {
				return true;
			}
		}
		return false;
	}

	private bool IsNotSelf(Transform current, Transform check) {
		foreach (Transform t in current) {
			if (t == check) { return false; }
			bool child = IsNotSelf(t, check);
			if (!child) { return child; }
		}
		return true;
	}
}
