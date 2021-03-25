using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPlanted : NetworkBehaviour
{
	[SerializeField] private AnimationCurve beepTime;
	[SerializeField] private AudioSource beepSound = null;
	[SerializeField] private GameObject explosion = null;
	[SerializeField] private float blastRadius = 60f;
	[SerializeField] private float blastDamage = 180f;
	[SerializeField] private int iconIndex = 0;

	private float nextBeepTime = 0;

	public bool defused = false;

	private UITimeManager timeManager;
	public UITimeManager TimeManager
	{
		get
		{
			if (timeManager != null) { return timeManager; }
			return timeManager = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UITimeManager>();
		}
	}

	private List<GameObject> Players
	{
		get
		{
			return PlayerSpawnSystem.players;
		}
	}

	private void Update()
	{
		if ((float)NetworkTime.time > nextBeepTime && !defused) {
			beepSound.Play();
			nextBeepTime = (float)NetworkTime.time + beepTime.Evaluate(TimeManager.bombEnd - (float)NetworkTime.time);
		}
	}

	[Server]
	public void BombExplode() {
		NetworkServer.Spawn(Instantiate(explosion, transform.position, transform.rotation));
		NetworkServer.Destroy(gameObject);
	}

	[Server]
	private void ExplosionDamage() {
		foreach (GameObject player in Players)
		{
			float dist = Vector3.Distance(player.transform.position, transform.position);
			if (dist <= blastRadius)
			{
				player.transform.GetComponent<PlayerHealthController>().RpcTakeDamage(((blastRadius - dist) / blastRadius) * blastDamage, null, iconIndex);
			}
		}
	}
}
