using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class UIKillFeedManager : NetworkBehaviour
{
	[SerializeField] private Sprite[] icons;
	[SerializeField] private GameObject template = null;
	[SerializeField] private float killFeedTime = 5f;

	[SerializeField] private Transform killFeedHolder = null;
	[SerializeField] public List<Transform> killFeedEntries;

	private float screenHeight = 0f;
	private float ScreenHeight
	{
		get
		{
			if (screenHeight != 0f) { return screenHeight; }
			return screenHeight = Screen.height;
		}
	}

	[Command (ignoreAuthority = true)]
	public void CmdAdd(string killer, string killed, int iconIndex, string killTeam, string deadTeam)
	{
		RpcAdd(killer, killed, iconIndex, killTeam, deadTeam);
	}

	[ClientRpc]
	public void RpcAdd(string killer, string killed, int iconIndex, string killTeam, string deadTeam)
	{
		GameObject killFeedInstance = Instantiate(template, killFeedHolder);
		killFeedInstance.GetComponent<KillFeedEntry>().SetInformation(killer, killed, icons[iconIndex], killTeam, deadTeam, this);
		killFeedEntries.Add(killFeedInstance.transform);
		OrderKillFeedEntries();
		Destroy(killFeedInstance, killFeedTime);
	}

	private void OrderKillFeedEntries()
	{
		for (int i = killFeedEntries.Count - 1; i >= 0; i--) {
			killFeedEntries[i].position = killFeedHolder.position - new Vector3(0, 35 * (ScreenHeight / 1080) * (killFeedEntries.Count - 1 - i), 0);
		}
	}
}
