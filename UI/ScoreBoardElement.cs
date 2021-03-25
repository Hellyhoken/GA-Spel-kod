using TMPro;
using UnityEngine;

public class ScoreBoardElement : MonoBehaviour
{
	[SerializeField] private TMP_Text nameText = null;
	[SerializeField] private TMP_Text killText = null;
	[SerializeField] private TMP_Text deathText = null;

	public void Set(NetworkGamePlayerLobby player) {
		nameText.text = player.displayName;
		killText.text = "" + player.kills;
		deathText.text = "" + player.deaths;
	}
}
