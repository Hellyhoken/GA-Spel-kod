using Fragsurf.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LegAnimation : NetworkBehaviour
{
	[SerializeField] private Transform leftLegTargetHolder = null;
	[SerializeField] private Transform rightLegTargetHolder = null;
	[SerializeField] private Transform leftLegIdleHolder = null;
	[SerializeField] private Transform rightLegIdleHolder = null;
	[SerializeField] private Animator animator = null;
	[SerializeField] private float stepLength = 1f;
	[Range(0,1)]
	[SerializeField] private float footOffset = 0.1f;
	[SerializeField] private LayerMask noWalkingMask;
	[SerializeField] private Transform hips = null;
	[SerializeField] private AnimationCurve footHeight;
	[SerializeField] private float maxFootHeight;
	[SerializeField] private Vector3 defaultHipOffset;
	[SerializeField] private Vector3 crouchingHipOffset;

	private Vector3 leftFootTargetPosition = Vector3.zero;
	private Vector3 rightFootTargetPosition = Vector3.zero;
	private Quaternion leftFootTargetRotation;
	private Quaternion rightFootTargetRotation;
	[SyncVar]
	private Vector3 leftFootPosition = Vector3.zero;
	[SyncVar]
	private Vector3 rightFootPosition = Vector3.zero;
	private Quaternion leftFootDefaultRotation;
	private Quaternion rightFootDefaultRotation;
	[SyncVar]
	private Quaternion leftFootRotation;
	[SyncVar]
	private Quaternion rightFootRotation;
	private Vector3 leftFootFinish = Vector3.zero;
	private Vector3 rightFootFinish = Vector3.zero;
	private Vector3 leftFootStart = Vector3.zero;
	private Vector3 rightFootStart = Vector3.zero;
	private Quaternion leftFootStartRotation;
	private Quaternion rightFootStartRotation;

	private bool leftLegUp = false;
	private bool rightLegUp = false;
	private bool prevWalk = false;
	private bool prevFly = false;

	private float leftFootDistance = 0f;
	private float rightFootDistance = 0f;
	private float leftFootCurrentDistance = 0f;
	private float rightFootCurrentDistance = 0f;

	private Vector3 lfp;
	private Vector3 rfp;
	private Quaternion lfr;
	private Quaternion rfr;
	private float currW;
	private float crouchPercent = 0f;
	private float crouchWant = 1f;

	[SyncVar]
	private float weight = 0;

	private SurfCharacter _surfer = null;
	[SerializeField] private float legMovementMultiplier = 0.5f;

	private SurfCharacter surfer
	{
		get
		{
			if (_surfer == null) { return _surfer = transform.root.GetComponent<SurfCharacter>(); }
			return _surfer;
		}
	}

	private void OnAnimatorIK(int layerIndex)
    {
		if (animator && layerIndex == 0 && transform.parent != null) {
			if (hasAuthority) {
				currW = ProccessWalking();
				CmdSetValues(lfp,rfp,lfr,rfr,currW);
			}
			animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
			animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
			animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
			animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);

			animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, weight);
			animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, weight);
			animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, weight);
			animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, weight);
		}
	}

	private void StartMovingLeftLeg() {
		leftLegUp = true;
		leftFootStart = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
		leftFootStartRotation = animator.GetIKRotation(AvatarIKGoal.LeftFoot);
		leftFootFinish = leftFootTargetPosition;
		leftFootDistance = Vector3.Distance(leftFootStart,leftFootFinish);
		leftFootCurrentDistance = 0f;
	}

	private void StartMovingRightLeg()
	{
		rightLegUp = true;
		rightFootStart = animator.GetIKPosition(AvatarIKGoal.RightFoot);
		rightFootStartRotation = animator.GetIKRotation(AvatarIKGoal.RightFoot);
		rightFootFinish = rightFootTargetPosition;
		rightFootDistance = Vector3.Distance(rightFootStart, rightFootFinish);
		rightFootCurrentDistance = 0f;
	}

	private float ProccessWalking() {
		bool walking = animator.GetBool("Walking");
		bool flying = animator.GetBool("Flying");
		bool crouching = animator.GetBool("Crouching");
		if (!walking || flying) {
			leftLegUp = false;
			rightLegUp = false;
		}
		if (!flying)
		{
			if (crouching)
			{
				crouchWant = 1f;
			}
			else
			{
				crouchWant = 0f;
			}
			crouchPercent = Mathf.Lerp(crouchPercent, crouchWant, surfer.moveData.crouchingSpeed * Time.deltaTime);
			hips.localPosition = Vector3.Lerp(defaultHipOffset, crouchingHipOffset, crouchPercent);
		}
		else {
			hips.localPosition = defaultHipOffset;
		}

		if (walking && !flying && !crouching && !prevFly)
		{
			// Walking animation

			hips.localPosition = defaultHipOffset;

			// Left Foot
			if (!rightLegUp)
			{
				if (!leftLegUp)
				{
					RaycastHit hit;
					// We cast our ray from above the foot in case the current terrain/floor is above the foot position.
					Ray ray = new Ray(leftLegTargetHolder.position + Vector3.up, Vector3.down);
					if (Physics.Raycast(ray, out hit, footOffset + 1f, ~noWalkingMask))
					{
						leftFootTargetPosition = hit.point;
						leftFootTargetPosition.y += footOffset;
						leftFootTargetRotation = Quaternion.LookRotation(hips.transform.forward, hit.normal);
					}
					else
					{
						leftFootTargetPosition = leftLegTargetHolder.position;
						leftFootTargetPosition.y += footOffset;
						leftFootTargetRotation = Quaternion.LookRotation(hips.transform.forward, transform.up);
					}
					if (!prevWalk || Vector3.Distance(leftFootTargetPosition, lfp) >= stepLength)
					{
						StartMovingLeftLeg();
					}
				}
				if (leftLegUp)
				{
					leftFootDefaultRotation = Quaternion.LookRotation(transform.forward,transform.up);
					leftFootCurrentDistance += Vector3.Scale(surfer.moveData.velocity, new Vector3(1, 0, 1)).magnitude * Time.deltaTime * legMovementMultiplier;
					float distPercent = leftFootCurrentDistance / leftFootDistance;
					lfp = Vector3.Lerp(leftFootStart, leftFootFinish, Mathf.Clamp(distPercent, 0, 1));
					float currentFootHeight = footHeight.Evaluate(Mathf.Clamp(distPercent, 0, 1));
					lfp.y += currentFootHeight * maxFootHeight;
					if (distPercent < 0.5)
					{
						lfr = Quaternion.Lerp(leftFootStartRotation, leftFootDefaultRotation, Mathf.Clamp(currentFootHeight, 0, 1));
					}
					else
					{
						lfr = Quaternion.Lerp(leftFootTargetRotation, leftFootDefaultRotation, Mathf.Clamp(currentFootHeight, 0, 1));
					}
					if (distPercent >= 1) { leftLegUp = false; }
				}
			}

			// Right Foot
			if (!leftLegUp)
			{
				if (!rightLegUp)
				{
					RaycastHit hit;
					// We cast our ray from above the foot in case the current terrain/floor is above the foot position.
					Ray ray = new Ray(rightLegTargetHolder.position + Vector3.up, Vector3.down);
					if (Physics.Raycast(ray, out hit, footOffset + 1f, ~noWalkingMask))
					{
						rightFootTargetPosition = hit.point;
						rightFootTargetPosition.y += footOffset;
						rightFootTargetRotation = Quaternion.LookRotation(hips.transform.forward, hit.normal);
					}
					else
					{
						rightFootTargetPosition = rightLegTargetHolder.position;
						rightFootTargetPosition.y += footOffset;
						rightFootTargetRotation = Quaternion.LookRotation(hips.transform.forward, transform.up);
					}
					if (Vector3.Distance(rightFootTargetPosition, rfp) >= stepLength)
					{
						StartMovingRightLeg();
					}
				}
				if (rightLegUp)
				{
					rightFootDefaultRotation = Quaternion.LookRotation(transform.forward, transform.up);
					rightFootCurrentDistance += Vector3.Scale(surfer.moveData.velocity, new Vector3(1, 0, 1)).magnitude * Time.deltaTime * legMovementMultiplier;
					float distPercent = rightFootCurrentDistance / rightFootDistance;
					rfp = Vector3.Lerp(rightFootStart, rightFootFinish, Mathf.Clamp(distPercent, 0, 1));
					float currentFootHeight = footHeight.Evaluate(Mathf.Clamp(distPercent, 0, 1));
					rfp.y += currentFootHeight * maxFootHeight;
					if (distPercent < 0.5)
					{
						rfr = Quaternion.Lerp(rightFootStartRotation, rightFootDefaultRotation, Mathf.Clamp(currentFootHeight, 0, 1));
					}
					else
					{
						rfr = Quaternion.Lerp(rightFootTargetRotation, rightFootDefaultRotation, Mathf.Clamp(currentFootHeight, 0, 1));
					}
					if (distPercent >= 1) { rightLegUp = false; }
				}
			}

			prevWalk = walking;
			prevFly = false;
			return 1f;
		}
		if ((!walking && !flying && !crouching) || (!flying && prevFly))
		{
			// Idle animation

			// Left Foot
			RaycastHit hit;
			// We cast our ray from above the foot in case the current terrain/floor is above the foot position.
			Ray ray = new Ray(leftLegIdleHolder.position + Vector3.up, Vector3.down);
			if (Physics.Raycast(ray, out hit, footOffset + 1f, ~noWalkingMask))
			{
				lfp = hit.point; // The target foot position is where the raycast hit a walkable object...
				lfp.y += footOffset; // ... taking account the distance to the ground we added above.
				lfr = Quaternion.LookRotation(hips.transform.forward, hit.normal);
			}
			else {
				lfp = leftLegIdleHolder.position;
				lfp.y += footOffset;
				lfr = Quaternion.LookRotation(hips.transform.forward, leftLegIdleHolder.up);
			}

			// Right Foot
			ray = new Ray(rightLegIdleHolder.position + Vector3.up, Vector3.down);
			if (Physics.Raycast(ray, out hit, footOffset + 1f, ~noWalkingMask))
			{
				rfp = hit.point;
				rfp.y += footOffset;
				rfr = Quaternion.LookRotation(hips.transform.forward, hit.normal);
			}
			else
			{
				rfp = leftLegIdleHolder.position;
				rfp.y += footOffset;
				rfr = Quaternion.LookRotation(hips.transform.forward, rightLegIdleHolder.up);
			}


			prevWalk = walking;
			prevFly = false;
			return 1f;
		}
		prevWalk = walking;
		prevFly = true;
		return 0;
	}

	[Command]
	private void CmdSetValues(Vector3 Lpos, Vector3 Rpos, Quaternion Lrot, Quaternion Rrot, float w) {
		leftFootPosition = Lpos;
		rightFootPosition = Rpos;
		leftFootRotation = Lrot;
		rightFootRotation = Rrot;
		weight = w;
	}
}
