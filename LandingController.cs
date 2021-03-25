using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingController : MonoBehaviour
{
	private AudioSource landingSound;

    // Start is called before the first frame update
    void Start()
    {
		landingSound = transform.GetComponent<AudioSource>();
    }

	public void playLanding(float speed) {
		if (Mathf.Abs(speed) < 9f && speed < 0)
		{
			landingSound.volume = Mathf.Abs(speed) / 9f;
		}
		else if (speed > 0) {
			landingSound.volume = 0f;
		}
		else
		{
			landingSound.volume = 1f;
		}
		landingSound.Play();
	}

}
