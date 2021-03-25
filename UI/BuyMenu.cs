using Mirror;
using System;
using UnityEngine;

public class BuyMenu : NetworkBehaviour
{
	[SerializeField] private GameObject buyMenu = null;
	[SerializeField] private MoneyHandler moneyHandler = null;

	[SerializeField] private BuyButton[] buyButtons;

	[SerializeField] private LayerMask buyZoneLayer;

	[SerializeField] private GameObject moneyPanel;

	public bool buyMenuOpen = false;
	public bool isInZone = false;

	public override void OnStartAuthority()
	{
		enabled = true;

		isInZone = Physics.CheckSphere(transform.position, 0.5f, buyZoneLayer);
		moneyPanel.SetActive(isInZone);

		InputManager.Controls.Player.Buy.performed += ctx => ToggleBuyMenu();
		InputManager.Controls.Player.CloseBuyMenu.performed += ctx => ToggleBuyMenu();
		InputManager.Add("CloseBuyMenu");
	}

	private void ToggleBuyMenu()
	{
		if (!isInZone) { return; }

		buyMenuOpen = !buyMenuOpen;

		buyMenu.SetActive(buyMenuOpen);

		if (buyMenuOpen)
		{
			Cursor.lockState = CursorLockMode.None;
			foreach (string disable in ActionNames.Buy)
			{
				InputManager.Add(disable);
			}
			InputManager.Remove("CloseBuyMenu");

			SetButtonState();

			return;
		}
		Cursor.lockState = CursorLockMode.Locked;
		foreach (string disable in ActionNames.Buy)
		{
			InputManager.Remove(disable);
		}
		InputManager.Add("CloseBuyMenu");
	}

	private void SetButtonState()
	{
		foreach (BuyButton button in buyButtons) {
			if (moneyHandler.money >= button.value)
			{
				button.button.interactable = true;
				button.valueText.color = Color.green;
			}
			else {
				button.button.interactable = false;
				button.valueText.color = Color.red;
			}
		}
	}

	public void Buy(BuyButton buyButton) {
		if (moneyHandler.money < buyButton.value) { return; }

		moneyHandler.ChangeMoney(-buyButton.value);

		SetButtonState();

		CmdSpawn(GetButtonIndex(buyButton));
	}

	[Command]
	private void CmdSpawn(int buttonIndex) {
		GameObject weapon = buyButtons[buttonIndex].weapon;
		NetworkServer.Spawn(Instantiate(weapon, transform.position + new Vector3(0,3f,0), transform.rotation));
	}

	private int GetButtonIndex(BuyButton buyButton) {
		for (int i = 0; i < buyButtons.Length; i++) {
			if (buyButton == buyButtons[i]) { return i; }
		}
		return 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!hasAuthority) { return; }

		if (other.gameObject.layer != 12) { return; }

		PlayerID player = transform.GetComponent<PlayerID>();
		if (player.isOrange && other.transform.tag == "GreenBuyZone") { return; }
		if (player.isGreen && other.transform.tag == "OrangeBuyZone") { return; }

		isInZone = true;

		moneyPanel.SetActive(true);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!hasAuthority) { return; }

		if (other.gameObject.layer != 12) { return; }

		PlayerID player = transform.GetComponent<PlayerID>();
		if (player.isOrange && other.transform.tag == "GreenBuyZone") { return; }
		if (player.isGreen && other.transform.tag == "OrangeBuyZone") { return; }

		isInZone = false;

		moneyPanel.SetActive(false);

		if (buyMenuOpen)
		{
			buyMenuOpen = !buyMenuOpen;

			buyMenu.SetActive(buyMenuOpen);

			Cursor.lockState = CursorLockMode.Locked;
			foreach (string disable in ActionNames.Buy)
			{
				InputManager.Remove(disable);
			}
		}
	}
}
