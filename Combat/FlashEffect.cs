using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashEffect : NetworkBehaviour
{

	[SerializeField] private float flashCutoffDist = 500f;
	[SerializeField] private float flashFadeDist = 300f;
	[SerializeField] private float flashCutoffAng = 155f;
	[SerializeField] private float flashFadeAng = 45f;

	[SerializeField] private float flashTime = 3f;
	[SerializeField] private float beginFadeTime = 0.8f;

	[SerializeField] private Image flashEffect;

	[SerializeField] private AudioSource flashBeep;

	[SerializeField] private LayerMask ignoreRayMask;

	private float currentFlashTime = 0f;

	private bool flashed = false;

	private Color flashColor = new Color(1,1,1,1);

    // Update is called once per frame
    void Update()
    {
		if (flashed) {
			flashEffect.enabled = true;
			currentFlashTime -= Time.deltaTime;
			if (currentFlashTime <= 0f)
			{
				flashEffect.enabled = false;
				flashBeep.Stop();
				flashed = false;
			}
			else if (currentFlashTime <= beginFadeTime)
			{
				flashColor.a = currentFlashTime / beginFadeTime;
				flashEffect.color = flashColor;
				flashBeep.volume = currentFlashTime / beginFadeTime;
			}
			else {
				flashColor.a = 1;
				flashEffect.color = flashColor;
				flashBeep.volume = 1;
			}
		}
    }

	private void BeginFlash(float flashAmount) {
		currentFlashTime = flashTime * flashAmount;
		flashBeep.Play();
		flashed = true;
	}

	[ClientRpc]
	public void RpcBeginFlash(Transform flashTrans) {
		if (!hasAuthority) { return; }
		Transform head = gameObject.FindComponentInChildWithTag<Collider>("Head").transform;
		float flashAmount = 1;
		if (head != null)
		{
			RaycastHit hit;
			Vector3 rayDirection = head.position - flashTrans.position;
			if (Physics.Raycast(flashTrans.position, rayDirection, out hit, Mathf.Infinity, ~ignoreRayMask))
			{
				if (hit.collider.transform == head)
				{
					bool done = false;
					if (Vector3.Distance(flashTrans.position, head.position) > flashCutoffDist)
					{
						flashAmount = 0;
						done = true;
					}
					else if (Vector3.Distance(flashTrans.position, head.position) < flashFadeDist)
					{
						flashAmount = 1;
					}
					else
					{
						flashAmount = (Vector3.Distance(flashTrans.position, head.position) - flashFadeDist) / (flashCutoffDist - flashFadeDist);
					}
					if (!done && Vector3.Angle(head.forward, -rayDirection) > flashCutoffAng)
					{
						flashAmount = 0;
					}
					else if (!done && Vector3.Angle(head.forward, -rayDirection) < flashFadeAng)
					{
						flashAmount *= 1;
					}
					else if (!done)
					{
						flashAmount *= Vector3.Angle(head.forward, -rayDirection) / (flashCutoffAng - flashFadeAng);
					}
					BeginFlash(flashAmount);
				}
			}
		}
	}
}
