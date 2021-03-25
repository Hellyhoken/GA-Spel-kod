using Mirror;
using TMPro;
using UnityEngine;

public class WinText : NetworkBehaviour
{
	[SerializeField] private Color orange;
	[SerializeField] private Color green;

	[SerializeField] private TMP_Text winText = null;
	[SerializeField] private GameObject scoreBoard = null;

	private RoundSystem roundSystem;
	public RoundSystem RoundSystem
	{
		get
		{
			if (roundSystem != null) { return roundSystem; }
			return roundSystem = GameObject.Find("RoundSystem(Clone)").GetComponent<RoundSystem>();
		}
	}

	public override void OnStartClient()
	{
		Setup(RoundSystem.gameWinner);
	}
	
	public void Setup(string winner) {
		scoreBoard.SetActive(true);
		if (winner == "O") {
			winText.color = orange;
			winText.text = "Orange won the game";
			return;
		}
		if (winner == "G") {
			winText.color = green;
			winText.text = "Green won the game";
			return;
		}
		if (winner == "D")
		{
			winText.text = "Draw";
			return;
		}
	}
}
