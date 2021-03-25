using UnityEngine;
using Mirror;

public class MoneyHandler : NetworkBehaviour
{
	[SyncVar(hook = nameof(HandleMoneyChanged))]
    public int money = 0;

	public NetworkGamePlayerLobby player = null;
	[SerializeField] private UIMoneyHandler moneyHandler = null;

	[Server]
	public void SetMoney(int value, NetworkGamePlayerLobby playerSet) {
		money = value;
		player = playerSet;
	}

	public void HandleMoneyChanged(int oldValue, int newValue) => UpdateScreen();

	private void UpdateScreen() {
		if (!hasAuthority) { return; }

		moneyHandler.UpdateMoney();
	}

	public void ChangeMoney(int change) {
		CmdChangeMoney(change);
    }

	[Command]
	private void CmdChangeMoney(int change) {
		money += change;
		player.CmdSetMoney(money);
	}
}
