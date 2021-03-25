using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeHolder : MonoBehaviour
{
	public GameObject[] grenades = new GameObject[4];

	public int currentGrenade = -1;

	private void OnDisable()
	{
		currentGrenade = -1;
	}

	public bool CycleGrenades(int change, bool button)
	{
		bool outRange = currentGrenade < 0 || currentGrenade > grenades.Length - 1;
		if (button && outRange) {
			currentGrenade = 0;
		}
		else if (!button && outRange) {
			if (change > 0)
			{
				currentGrenade = -1;
			}
			else if (change < 0) {
				currentGrenade = grenades.Length;
			}
		}

		currentGrenade = FindNextGrenade(currentGrenade, change, button);

		outRange = currentGrenade < 0 || currentGrenade > grenades.Length - 1;
		if (outRange)
		{
			return true;
		}
		else {
			SelectGrenade();
			return false;
		}
	}

	public int FindNextGrenade(int curGrenade, int move, bool button)
	{
		if (button && curGrenade == grenades.Length - 1)
		{
			curGrenade = 0;
		}
		else
		{
			curGrenade += move;
		}

		bool outRange = curGrenade < 0 || curGrenade > grenades.Length - 1;
		if (!button && outRange) {
			return curGrenade;
		}
		else if (grenades[curGrenade] == null)
		{
			return FindNextGrenade(curGrenade, move, button);
		}
		else
		{
			return curGrenade;
		}
	}

	public bool HasGrenade() {
		foreach (GameObject grenade in grenades) {
			if (grenade != null) {
				return true;
			}
		}
		return false;
	}

	public void SelectGrenade()
	{
		foreach (GameObject i in grenades)
		{
			if (i != null)
			{
				i.SetActive(false);
			}
		}
		grenades[currentGrenade].SetActive(true);
	}
}
