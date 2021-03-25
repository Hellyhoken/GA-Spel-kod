using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class UITimeManager : NetworkBehaviour
{
	[SerializeField] private TMP_Text timeText = null;
	[SerializeField] private Image bombImage = null;
	[SerializeField] private int freezeTime = 15;
	[SerializeField] private int roundTime = 115;
	[SerializeField] private int bombTime = 40;

	private RoundSystem roundSystem = null;

	private bool isFreeze = false;
	public bool isRound = false;
	public bool isBomb = false;

	private float freezeEnd = 0f;
	private float roundEnd = 0f;
	public float bombEnd = 0f;

	private void Update()
	{
		if (isFreeze)
		{
			timeText.gameObject.SetActive(true);
			bombImage.gameObject.SetActive(false);
			float time = freezeEnd - (float)NetworkTime.time;
			if (time <= 0f) {
				isFreeze = false;
				roundSystem.StartRound();
				return;
			}
			int minutes = (int)((int)time / 60);
			int seconds = (int)time - minutes * 60;
			string secs = seconds < 10 ? 
				"0" + seconds : 
				"" + seconds;
			timeText.text = (int)time <= (int)(freezeTime * 2 / 3) ?
				$"<color=red>{minutes}:" + secs + "</color>" :
				$"<color=white>{minutes}:" + secs + "</color>";
		}
		if (isBomb) {
			timeText.gameObject.SetActive(false);
			bombImage.gameObject.SetActive(true);
			if ((float)NetworkTime.time >= bombEnd)
			{
				BombExplode();
			}
			return;
		}
		if (isRound)
		{
			timeText.gameObject.SetActive(true);
			bombImage.gameObject.SetActive(false);
			float time = roundEnd - (float)NetworkTime.time;
			int minutes = (int)(time / 60);
			int seconds = (int)time - minutes * 60;
			string secs = seconds < 10 ?
				"0" + seconds :
				"" + seconds;
			timeText.text = (int)time <= freezeTime ?
				$"<color=red>{minutes}:" + secs + "</color>" :
				$"<color=white>{minutes}:" + secs + "</color>";
			if (time <= 0f) {
				EndRound("G");
			}
		}
	}

	public void StartFreeze(RoundSystem roundSys) {
		roundSystem = roundSys;
		isFreeze = true;
		freezeEnd = (float)NetworkTime.time + freezeTime;
	}

	public void StartRound()
	{
		isRound = true;
		roundEnd = (float)NetworkTime.time + roundTime;
	}

	[ServerCallback]
	public void EndRound(string winner) {
		isRound = false;
		roundSystem.EndRound(winner);
		RpcEndRound();
	}

	[ClientRpc]
	private void RpcEndRound() {
		isRound = false;
		isBomb = false;
	}

	public void PlantBomb() {
		isBomb = true;
		bombEnd = (float)NetworkTime.time + bombTime;
		Debug.Log(bombEnd);
	}

	[ServerCallback]
	private void BombExplode() {
		roundSystem.BombExplode();
	}
}
