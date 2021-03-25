using Mirror;
using TMPro;
using UnityEngine;

public class UIRoundManager : NetworkBehaviour
{
	[SerializeField] private TMP_Text green = null;
	[SerializeField] private TMP_Text orange = null;

	[SerializeField] private TMP_Text winText = null;
	[SerializeField] private GameObject winPanel = null;

	[SerializeField] private Color greenColor;
	[SerializeField] private Color orangeColor;

	private void Awake() => DontDestroyOnLoad(this.gameObject);

	[ClientRpc]
	public void RpcSetText(int ora, int gre) {
		green.text = gre.ToString();
		orange.text = ora.ToString();
	}

	[ClientRpc]
	public void RpcShowWin(string winner) {
		if (winner == "O") {
			winText.color = orangeColor;
			winText.text = "Orange won the round";
			winPanel.SetActive(true);
			return;
		}
		if (winner == "G") {
			winText.color = greenColor;
			winText.text = "Green won the round";
			winPanel.SetActive(true);
			return;
		}
		winText.color = Color.white;
		winText.text = "Draw";
		winPanel.SetActive(true);
	}

	[ClientRpc]
	public void RpcHideWin() {
		winPanel.SetActive(false);
	}
}
