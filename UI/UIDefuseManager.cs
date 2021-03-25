using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDefuseManager : MonoBehaviour
{
	[SerializeField] private Slider defuseSlide = null;
	[SerializeField] private GameObject defuse = null;

	public void Defusing(float value) {
		defuseSlide.value = value;
		defuse.SetActive(true);
	}

	public void StopDefusing()
	{
		defuseSlide.value = 0f;
		defuse.SetActive(false);
	}
}
