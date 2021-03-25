using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
	[SerializeField] private NetworkManagerLobby networkManager = null;

	[SerializeField] private GameObject landingPagePanel = null;
	[SerializeField] private TMP_InputField ipAdressInputField = null;
	[SerializeField] private Button joinButton = null;

	[SerializeField] private GameObject ipField = null;

	private void Start()
	{
		NetworkManagerLobby.OnClientConnected += HandleClientConnected;
		NetworkManagerLobby.OnClientDisconnected += HandleClientDisconnected;
		NetworkManagerLobby.OnServerStopped += StopJoining;
	}

	public void JoinLobby() {
		string ipAdress = ipAdressInputField.text;

		if (string.IsNullOrEmpty(ipAdress)) { return; }

		networkManager.networkAddress = ipAdress;
		networkManager.StartClient();

		joinButton.interactable = false;
	}

	private void HandleClientConnected() {
		joinButton.interactable = true;

		gameObject.SetActive(false);
		landingPagePanel.SetActive(false);
	}

	private void HandleClientDisconnected() {
		joinButton.interactable = true;
	}

	public void StopJoining() {
		networkManager.StopClient();
		joinButton.interactable = true;
		ipField.SetActive(false);
	}
}
