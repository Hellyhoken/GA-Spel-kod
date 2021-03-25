using Mirror;
using UnityEngine;

public class PauseController : NetworkBehaviour
{
	[SerializeField] private GameObject pauseUI = null;
	[SerializeField] private GameObject[] extraPanels = null;
	[SerializeField] private PlayerHealthController healthController = null;

	private bool paused = false;

	public override void OnStartAuthority()
	{
		enabled = true;

		InputManager.Controls.Player.Pause.performed += ctx => Pause();
	}

	public void Pause() {
		paused = !paused;
		pauseUI.SetActive(paused);
		if (paused) {
			Cursor.lockState = CursorLockMode.None;
			foreach (string disable in ActionNames.Pause)
			{
				InputManager.Add(disable);
			}
			return;
		}
		Cursor.lockState = CursorLockMode.Locked;
		foreach (string disable in ActionNames.Pause)
		{
			InputManager.Remove(disable);
		}
		foreach (GameObject panel in extraPanels) { panel.SetActive(false); }
	}

	public void Quit()
	{
		healthController.SuicideToQuitGame();
		Application.Quit();
	}
}
