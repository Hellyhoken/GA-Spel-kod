using UnityEngine;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private NetworkManagerLobby networkManager = null;

	[SerializeField] private GameObject landingPagePanel = null;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	public void HostLobby() {
		networkManager.StartHost();

		landingPagePanel.SetActive(false);
	}
}
