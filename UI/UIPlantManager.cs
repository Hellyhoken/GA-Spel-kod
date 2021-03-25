using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlantManager : MonoBehaviour
{
	[SerializeField] private Slider plantSlide = null;
	[SerializeField] private GameObject plant = null;

	public void Planting(float value) {
		plantSlide.value = value;
		plant.SetActive(true);
	}

	public void StopPlanting()
	{
		plantSlide.value = 0f;
		plant.SetActive(false);
	}
}
