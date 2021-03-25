using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrown : NetworkBehaviour
{
	public bool isGrenade;
	public bool isFlash;
	public bool isSmoke;

	public float explodeTime;
	public float groundDistance;
	public float blastDamage;
	public float blastRadius;
	public GameObject playerThrown = null;

	public GameObject explodeParticle;
	public GameObject explosionSound;

	public Rigidbody rb = null;
	public AudioSource hitSound = null;

	public LayerMask groundMask;
	public LayerMask playerMask;

	bool smokeActivate = false;
	bool explode = false;
	[SerializeField] private int iconIndex = 0;

	[SerializeField] private LayerMask ignoreRayMask;

	[SerializeField] private Component[] destroyOnExplode;
	
	private List<GameObject> Players {
		get {
			return PlayerSpawnSystem.players;
		}
	}

	[ServerCallback]
	public void GrenadeTimer(Vector3 force, GameObject thrower)
	{
		playerThrown = thrower;
		rb.AddForce(force.x * transform.right + force.y * transform.up + force.z * transform.forward, ForceMode.VelocityChange);
		if (isSmoke)
		{
			StartCoroutine(ActivateSmoke());
		}
		else
		{
			StartCoroutine(GrenadeTime());
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.tag != "Player")
		{
			hitSound.Play();
		}
	}

	[ServerCallback]
	private void Update()
	{
		bool isGrounded = Physics.CheckSphere(transform.position, groundDistance, groundMask);
		if (isGrounded && rb != null)
		{
			rb.angularDrag = 10f;
		}
		else if (rb != null)
		{
			rb.angularDrag = 1f;
		}
		if (smokeActivate && Vector3.Distance(rb.velocity, new Vector3(0, 0, 0)) < 0.2f && Vector3.Distance(rb.velocity, new Vector3(0, 0, 0)) > -0.2f)
		{
			SmokeExplode();
		}
		else if (explode && isFlash)
		{
			FlashExplode();
		}
		else if (explode && isGrenade)
		{
			GrenadeExplode(isGrounded);
		}
	}

	[Server]
	void SmokeExplode()
	{
		smokeActivate = false;
		GameObject particle = Instantiate(explodeParticle, transform.position, Quaternion.Euler(0, 0, 0));
		NetworkServer.Spawn(particle);

		foreach (Component component in destroyOnExplode) {
			Destroy(component);
		}
		RpcDestroy();
		StartCoroutine(SmokeDestroy(particle));
	}

	[Server]
	private IEnumerator SmokeDestroy(GameObject particle) {
		yield return new WaitForSeconds(16);
		NetworkServer.Destroy(particle);
		NetworkServer.Destroy(gameObject);
	}

	[Server]
	void FlashExplode()
	{
		explode = false;
		GameObject sound = Instantiate(explosionSound, transform.position, transform.rotation);
		NetworkServer.Spawn(sound);

		foreach (GameObject player in Players)
		{
			player.GetComponent<FlashEffect>().RpcBeginFlash(transform);
		}
		foreach (Component component in destroyOnExplode)
		{
			Destroy(component);
		}
		RpcDestroy();
		StartCoroutine(FlashDestroy(sound));
	}

	[Server]
	private IEnumerator FlashDestroy(GameObject sound)
	{
		yield return new WaitForSeconds(2);
		NetworkServer.Destroy(sound);
		NetworkServer.Destroy(gameObject);
	}

	[Server]
	void GrenadeExplode(bool isGrounded)
	{
		explode = false;
		GameObject particle = Instantiate(explodeParticle, transform.position, Quaternion.Euler(0, 0, 0));
		NetworkServer.Spawn(particle);

		GameObject sound = Instantiate(explosionSound, transform.position, transform.rotation);
		NetworkServer.Spawn(sound);

		foreach (GameObject player in Players)
		{
			foreach (Transform bodyPart in GetBodyParts(player.FindComponentInChildWithTag<Transform>("Character"))) {
				float dist = Vector3.Distance(transform.position, bodyPart.position);
				if (dist <= blastRadius)
				{
					RaycastHit hit;
					Vector3 rayDirection = bodyPart.position - transform.position;
					if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity, ~ignoreRayMask))
					{
						if (hit.collider.transform == bodyPart)
						{
							player.transform.GetComponent<PlayerHealthController>().RpcTakeDamage(((blastRadius - dist) / blastRadius) * blastDamage, playerThrown, iconIndex);
						}
						break;
					}
				}
			}
		}
		foreach (Component component in destroyOnExplode)
		{
			Destroy(component);
		}
		RpcDestroy();
		StartCoroutine(GrenadeDestroy(particle, sound));
	}

	private List<Transform> GetBodyParts(Transform parent) {
		List<Transform> output = new List<Transform>();
		foreach (Transform part in parent) {
			if (part.GetComponent<Collider>() != null) { output.Add(part); }
			if (part.childCount > 0) {
				foreach (Transform get in GetBodyParts(part)) {
					output.Add(get);
				}
			}
		}
		return output;
	}

	[Server]
	private IEnumerator GrenadeDestroy(GameObject particle, GameObject sound) {
		yield return new WaitForSeconds(3);
		NetworkServer.Destroy(sound);
		yield return new WaitForSeconds(2);
		NetworkServer.Destroy(particle);
		NetworkServer.Destroy(gameObject);
	}

	[Server]
	private IEnumerator ActivateSmoke()
	{
		yield return new WaitForSeconds(0.1f);
		smokeActivate = true;
	}

	[Server]
	private IEnumerator GrenadeTime()
	{
		yield return new WaitForSeconds(explodeTime);
		explode = true;
	}

	[ClientRpc]
	private void RpcDestroy() {
		foreach (Component component in destroyOnExplode)
		{
			Destroy(component);
		}
	}
}
