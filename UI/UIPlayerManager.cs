using Mirror;
using TMPro;
using UnityEngine;

public class UIPlayerManager : NetworkBehaviour
{
	[SerializeField] private TMP_Text green = null;
	[SerializeField] private TMP_Text orange = null;

	[ClientRpc]
	public void RpcSetText(int ora, int gre)
	{
		green.text = gre.ToString();
		orange.text = ora.ToString();
	}
}
