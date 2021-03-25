using Mirror;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpectatorScreenController : NetworkBehaviour
{
	[SerializeField] private GameObject[] tvs;
	[SerializeField] private Shader shader = null;
	[SerializeField] private Color color;
	[SerializeField] private Material deadMaterial = null;

	public List<GameObject> players = new List<GameObject>();

	[Server]
	public void AddPlayer(GameObject player) {
		players.Add(player);
		RpcAddPlayer(players.ToArray());
	}

	[ClientRpc]
	private void RpcAddPlayer(GameObject[] plays) {
		players = plays.ToList();
		StartScreens();
	}

	private void StartScreens()
	{
		for (int i = 0; i < players.Count; i++)
		{
			Renderer screen = tvs[i].transform.Find("Screen").GetComponent<Renderer>();

			screen.material = new Material(shader);
			RenderTexture rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);

			players[i].GetComponentInChildren<Camera>().targetTexture = rt;

			screen.material.mainTexture = rt;
			screen.material.color = color;

			tvs[i].GetComponentInChildren<TMP_Text>().text = players[i].GetComponent<PlayerID>().displayName;
		}
	}

	[Command(ignoreAuthority = true)]
	public void CmdKillPlayer(string playerName)
	{
		RpcKillPlayer(playerName);
	}

	[ClientRpc]
	private void RpcKillPlayer(string playerName)
	{
		for (int i = 0; i < players.Count; i++) {
			if (players[i].GetComponent<PlayerID>().displayName == playerName)
			{
				Renderer screen = tvs[i].transform.Find("Screen").GetComponent<Renderer>();

				screen.material = deadMaterial;

				return;
			}
		}
	}
}
