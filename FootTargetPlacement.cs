using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Movement;
using Mirror;

public class FootTargetPlacement : NetworkBehaviour
{
	[SerializeField] private float footRadius = 0.5f;
	[SerializeField] private float footSpacing = 0.2f;
	[SerializeField] private Transform walkRightFoot = null;
	[SerializeField] private Transform walkLeftFoot = null;

	private SurfCharacter _surfer = null;
	private SurfCharacter surfer {
		get {
			if (_surfer == null) { return _surfer = transform.root.GetComponent<SurfCharacter>(); }
			return _surfer;
		}
	}

	public override void OnStartAuthority()
	{
		enabled = true;
	}

	// Update is called once per frame
	void Update()
    {
		if (transform.parent != null) {
			Vector3 velocityDir = Vector3.Scale(surfer.moveData.velocity, new Vector3(1f,0,1f)).normalized;
			Vector3 middle = transform.position + velocityDir * footRadius;
			Vector3 offsetDir = Vector3.Cross(velocityDir, Vector3.up).normalized;
			Vector3 offset = offsetDir * footSpacing;
			if (Vector3.Angle(velocityDir, transform.forward) > 90)
			{
				walkLeftFoot.position = middle - offset;
				walkRightFoot.position = middle + offset;
			}
			else {
				walkLeftFoot.position = middle + offset;
				walkRightFoot.position = middle - offset;
			}
		}
    }
}
