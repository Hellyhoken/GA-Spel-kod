using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScoreBoardManager : MonoBehaviour
{
	[SerializeField] private TMP_Text orangePointsText = null;
	[SerializeField] private TMP_Text greenPointsText = null;
	[SerializeField] private Transform orangePlayersHolder = null;
	[SerializeField] private Transform greenPlayersHolder = null;
	[SerializeField] private GameObject playerTemplate = null;

	private List<NetworkGamePlayerLobby> orangePlayers = new List<NetworkGamePlayerLobby>();
	private List<NetworkGamePlayerLobby> greenPlayers = new List<NetworkGamePlayerLobby>();
	private int orangePoints = 0;
	private int greenPoints = 0;

	private RoundSystem roundSystem;
	public RoundSystem RoundSystem
	{
		get
		{
			if (roundSystem != null) { return roundSystem; }
			return roundSystem = GameObject.Find("RoundSystem(Clone)").GetComponent<RoundSystem>();
		}
	}

	private float screenHeight = 0f;
	private float ScreenHeight
	{
		get
		{
			if (screenHeight != 0f) { return screenHeight; }
			return screenHeight = Screen.height;
		}
	}

	private void OnEnable()
	{
		GetInfo();
		SetInfo();
	}

	public void Close()
	{
		foreach (Transform player in orangePlayersHolder.transform) {
			Destroy(player.gameObject);
		}
		foreach (Transform player in greenPlayersHolder.transform)
		{
			Destroy(player.gameObject);
		}
		greenPlayers.Clear();
		orangePlayers.Clear();
		gameObject.SetActive(false);
	}

	private void GetInfo()
	{
		foreach (NetworkGamePlayerLobby player in RoundSystem.Room.GamePlayers)
		{
			if (player.IsGreen)
			{
				greenPlayers.Add(player);
			}
			else if (player.IsOrange)
			{
				orangePlayers.Add(player);
			}
		}
		greenPlayers.Sort(SortByKills);
		orangePlayers.Sort(SortByKills);
		orangePoints = RoundSystem.orangePoints;
		greenPoints = RoundSystem.greenPoints;
	}

	private void SetInfo()
	{
		orangePointsText.text = "" + orangePoints;
		greenPointsText.text = "" + greenPoints;

		for (int i = 0; i < greenPlayers.Count; i++) {
			GameObject newPlayer = Instantiate(playerTemplate, greenPlayersHolder);
			newPlayer.transform.position -= new Vector3(0, 35 * (ScreenHeight / 1080) * i, 0);
			newPlayer.GetComponent<ScoreBoardElement>().Set(greenPlayers[i]);
		}
		for (int i = 0; i < orangePlayers.Count; i++) {
			GameObject newPlayer = Instantiate(playerTemplate, orangePlayersHolder);
			newPlayer.transform.position -= new Vector3(0, 35 * (ScreenHeight / 1080) * i, 0);
			newPlayer.GetComponent<ScoreBoardElement>().Set(orangePlayers[i]);
		}
	}

	private int SortByKills(NetworkGamePlayerLobby p1, NetworkGamePlayerLobby p2) {
		return p1.kills.CompareTo(p2.kills);
	}
}
