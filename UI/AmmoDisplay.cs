using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : NetworkBehaviour
{
	[SerializeField] private GameObject hud;
	[SerializeField] private TMP_Text ammoText = null;

	public override void OnStartAuthority()
	{
		hud.SetActive(true);
	}

	public void UpdateDisplay(int ammo, int reserve) {
		if (!hasAuthority) { return; }
		ammoText.text = $"{ammo}/{reserve}";
	}

	public void UnSetDisplay() {
		if (!hasAuthority) { return; }
		ammoText.text = "-/-";
	}
}
