using Mirror;
using UnityEngine;

public class ScoreBoardOpener : NetworkBehaviour
{
	[SerializeField] private GameObject ScoreBoard = null;

	public override void OnStartAuthority() {
		enabled = true;

		InputManager.Controls.Player.ScoreBoard.performed += ctx => ScoreBoard.SetActive(true);

		InputManager.Controls.Player.ScoreBoard.canceled += ctx => ScoreBoard.GetComponent<UIScoreBoardManager>().Close();
	}
}
