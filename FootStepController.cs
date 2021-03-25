using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepController : MonoBehaviour
{
	[SerializeField] private AudioSource[] footSteps;

	private void PlayFootStep() {
		footSteps[Random.Range(0, footSteps.Length)].Play();
	}
}
