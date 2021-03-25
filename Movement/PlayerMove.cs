using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
	[SerializeField] private CharacterController controller = null;
	[SerializeField] private float moveAcc = 5f;
	[SerializeField] private PlayerGravity playerGravity = null;
	[SerializeField] private float max_velocity = 5f;
	//[SerializeField] public float max_velocity_ground = 5f;
	//[SerializeField] private float max_velocity_air = 5f;
	[SerializeField] private float ground_accelerate = 5.5f;
	[SerializeField] private float air_accelerate = 12f;
	[SerializeField] private float friction = 1f;

	[HideInInspector]
	public float crouchMultiplier = 1f;

	public Vector3 velocity = Vector3.zero;
	private Vector3 distance = Vector3.zero;
	private bool grounded = true;

	public override void OnStartAuthority()
	{
		enabled = true;
	}

	[ClientCallback]
	private void Update()
	{
		if (grounded && !playerGravity.isGrounded)
		{
			grounded = false;
		}

		ProcessMovement();

		controller.Move(distance);
		//if (grounded) { GroundMove(); }
		//else { AirMove(); }
		if (!grounded && playerGravity.isGrounded)
		{
			grounded = true;
		}
	}

	private void ProcessMovement()
	{
		if (!grounded)
		{

		}
	}

}
