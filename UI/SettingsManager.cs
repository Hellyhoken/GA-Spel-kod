using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	private string playerPrefsSensKey = "Sensitivity";
	private float defaultSensitivity = 10f;

	[SerializeField] private Slider sensitivitySlider = null;
	[SerializeField] private TMP_InputField sensitivityField = null;
	[SerializeField] private PlayerCameraController cameraController = null;

	private void Awake()
	{
		SetupSens();
	}

	public void SetSensitivity(bool slider) {
		float sens;
		if (slider) {
			sens = sensitivitySlider.value / 100;
			sensitivityField.text = $"{sens}";
			cameraController.mouseSensitivity = sens * 10f;
			PlayerPrefs.SetFloat(playerPrefsSensKey, sens);
			return;
		}
		sens = float.Parse(sensitivityField.text);
		sensitivitySlider.value = Mathf.Clamp(sens, sensitivitySlider.minValue, sensitivitySlider.maxValue);
		cameraController.mouseSensitivity = sens * 10f;
		PlayerPrefs.SetFloat(playerPrefsSensKey, sens);
	}

	public void SetupSens() {
		float sens = PlayerPrefs.GetFloat(playerPrefsSensKey, defaultSensitivity);
		sensitivitySlider.value = sens * 100;
		sensitivityField.text = $"{sens}";
		cameraController.mouseSensitivity = sens * 10f;
	}
}
