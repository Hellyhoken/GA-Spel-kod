using Mirror;
using TMPro;
using UnityEngine;

public class UIMoneyHandler : NetworkBehaviour
{
	[SerializeField] private TMP_Text moneyText;

	private void OnEnable()
	{
		UpdateMoney();
	}

	public void UpdateMoney()
	{
		moneyText.text = "$ " + transform.root.GetComponent<MoneyHandler>().money;
	}
}
