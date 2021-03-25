using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
	[SerializeField] private string team = "";

	private void Awake() => PlayerSpawnSystem.AddSpawnPoint(transform, team);

	private void OnDrawGizmos()
	{
		Gizmos.color = string.IsNullOrEmpty(team) ?
			Color.white :
			team == "G" ?
			Color.green :
			Color.Lerp(Color.yellow, Color.red, 0.5f);
		Gizmos.DrawSphere(transform.position, 1f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
	}
}
