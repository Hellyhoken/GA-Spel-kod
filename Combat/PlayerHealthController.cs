using Mirror;
using UnityEngine;

public class PlayerHealthController : NetworkBehaviour
{
	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private PlayerID playerID = null;
	[SerializeField] private WeaponSelect weaponSelect = null;
	[SerializeField] private float friendlyFireMultiplier = 0.5f;
	[SerializeField] private UIHealthManager healthDisplay = null;
	[SyncVar]
	private float health = 100f;
	private bool dead = false;

	private UIKillFeedManager killFeed = null;
	private UIKillFeedManager KillFeed {
		get {
			if (killFeed != null) { return killFeed; }
			return killFeed = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UIKillFeedManager>();
		}
	}

	[ClientRpc]
	public void RpcTakeDamage(float damage, GameObject shooter, int iconIndex) {
		if (!hasAuthority) { return; }
		string team = playerID.isGreen ? "G" : "O";
		PlayerID shooterID;
		if (shooter != null)
		{
			shooterID = shooter.GetComponent<PlayerID>();
			string shooterTeam = shooterID.isGreen ? "G" : "O";
			health = Mathf.Max(team == shooterTeam ? health - damage * friendlyFireMultiplier : health - damage, 0f);
		}
		else {
			shooterID = null;
			health = Mathf.Max(health - damage, 0f);
		}
		healthDisplay.SetHealth((int)health);
		if (health <= 0f) {
			Die(shooterID, iconIndex);
		}
	}

	private void Die(PlayerID shooter, int iconIndex) {
		if (dead) { return; }
		dead = true;
		string deadTeam = playerID.isGreen ? "G" : "O";
		CmdPlayerDied(deadTeam);
		if (playerID != shooter && shooter != null)
		{
			shooter.CmdAddKill();
			playerID.CmdAddDeath();
			weaponSelect.RemoveWeapons();
			DeathRoom();
		}
		else if (shooter == null)
		{
			playerID.CmdAddDeath();
			weaponSelect.RemoveWeapons();
			DeathRoom();

			KillFeed.CmdAdd("", playerID.displayName, iconIndex, "O", deadTeam);
			return;
		}
		string killTeam = shooter.isGreen ? "G" : "O";
		KillFeed.CmdAdd(shooter.displayName, playerID.displayName, iconIndex, killTeam, deadTeam);
	}

	private void DeathRoom()
	{
		if (playerID.isOrange)
		{
			GameObject deadRoomO = GameObject.Find("OrangeDeadRoom");
			transform.position = deadRoomO.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
			deadRoomO.GetComponent<SpectatorScreenController>().CmdKillPlayer(playerID.displayName);
			return;
		}
		GameObject deadRoomG = GameObject.Find("GreenDeadRoom");
		transform.position = deadRoomG.transform.position + new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
		deadRoomG.GetComponent<SpectatorScreenController>().CmdKillPlayer(playerID.displayName);
	}

	[Command]
	private void CmdPlayerDied(string team) {
		GameObject.Find("RoundSystem(Clone)").GetComponent<RoundSystem>().PlayerDied(team);
	}

	public void SuicideToQuitGame()
	{
		Die(playerID, 4);
	}
}
