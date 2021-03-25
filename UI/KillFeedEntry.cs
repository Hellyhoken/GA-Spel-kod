using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedEntry : MonoBehaviour
{
	[SerializeField] private TMP_Text killerText = null;
	[SerializeField] private TMP_Text killedText = null;
	[SerializeField] private Image weaponIcon = null;

	[SerializeField] private Color orange;
	[SerializeField] private Color green;

	private UIKillFeedManager feedManager = null;

	public void SetInformation(string killer, string killed, Sprite icon, string killTeam, string deadTeam, UIKillFeedManager killFeedManager) {
		killerText.color = killTeam == "G" ? green : orange;
		killedText.color = deadTeam == "G" ? green : orange;
		killerText.text = killer;
		killedText.text = killed;
		weaponIcon.sprite = icon;
		feedManager = killFeedManager;
	}

	private void OnDestroy()
	{
		feedManager.killFeedEntries.Remove(transform);
	}
}
