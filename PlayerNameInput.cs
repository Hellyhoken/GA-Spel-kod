using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] private TMP_InputField nameInputField = null;
	[SerializeField] private Button continueButton = null;

	public static string DisplayName { get; private set; }

	private const string PlayerPrefsNameKey = "PlayerName";

	private void Start() => SetupInputField();

	private void SetupInputField() {
		if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) {
			SetPlayerName("");
			return;
		}

		string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

		nameInputField.text = defaultName;

		SetPlayerName(defaultName);
	}

	public void SetPlayerName(string name) {
		if (string.IsNullOrEmpty(name)) { name = nameInputField.text; }

		continueButton.interactable = !string.IsNullOrEmpty(name);
	}

	public void SavePlayerName() {
		DisplayName = nameInputField.text;

		PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
	}
}
