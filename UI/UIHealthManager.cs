using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthManager : MonoBehaviour
{
	[SerializeField] private TMP_Text text = null;
	[SerializeField] private Slider slider = null;

	public void SetHealth(int health)
	{
		text.text = $"{health}";
		slider.value = health;
	}
}
