using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{
	[SerializeField] private GameObject lobbyUI = null;
	[SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[10];
	[SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[10];
	[SerializeField] private RectTransform[] playerTeams = new RectTransform[10];
	[SerializeField] private RectTransform orangeButton = null;
	[SerializeField] private RectTransform greenButton = null;
	[SerializeField] private Button startGameButton = null;

	[SyncVar(hook = nameof(HandleDisplayNameChanged))]
	public string DisplayName = "Loading...";
	[SyncVar(hook = nameof(HandleReadyStatusChanged))]
	public bool IsReady = false;
	[SyncVar(hook = nameof(HandleTeamChanged))]
	public string Team = "";

	private float screenWidth = 0f;

	private float ScreenWidth {
		get {
			if (screenWidth != 0f) { return screenWidth; }
			return screenWidth = Screen.width;
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

	private bool isLeader;

	public bool IsLeader {
		set {
			isLeader = value;
			startGameButton.gameObject.SetActive(value);
		}
	}

	private NetworkManagerLobby room;

	public NetworkManagerLobby Room {
		get {
			if (room != null) { return room; }
			return room = NetworkManager.singleton as NetworkManagerLobby;
		}
	}

	public override void OnStartAuthority()
	{
		CmdSetDisplayName(PlayerNameInput.DisplayName);

		Team = "";

		lobbyUI.SetActive(true);
	}

	public override void OnStartClient()
	{
		Room.RoomPlayers.Add(this);

		UpdateDisplay();
	}

	public override void OnNetworkDestroy()
	{
		Room.RoomPlayers.Remove(this);

		UpdateDisplay();
	}

	public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();

	public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

	public void HandleTeamChanged(string oldValue, string newValue) => UpdateDisplay();

	private void UpdateDisplay() {
		if (!hasAuthority) {
			foreach (var player in Room.RoomPlayers) {
				if (player.hasAuthority) {
					player.UpdateDisplay();
					break;
				}
			}
			return;
		}

		for (int i = 0; i < playerNameTexts.Length; i++) {
			playerNameTexts[i].text = "Waiting for player...";
			playerReadyTexts[i].text = string.Empty;
			playerTeams[i].position = new Vector3(ScreenWidth / 2, greenButton.position.y - 70 * (ScreenHeight / 1080) * (i + 1), 0);
		}

		for (int i = 0; i < Room.RoomPlayers.Count; i++) {
			playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
			playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
				"<color=green>Ready</color>" :
				"<color=red>Not Ready</color>";
			string team = Room.RoomPlayers[i].Team;
			if (string.IsNullOrEmpty(team)) { playerTeams[i].position = new Vector3((ScreenWidth / 2), orangeButton.position.y - 70 * (ScreenHeight / 1080) * (i + 1), 0); }
			else if (team == "O") { playerTeams[i].position = new Vector3(orangeButton.position.x, orangeButton.position.y - 70 * (ScreenHeight / 1080) * (i + 1), 0); }
			else if (team == "G") { playerTeams[i].position = new Vector3(greenButton.position.x, greenButton.position.y - 70 * (ScreenHeight / 1080) * (i + 1), 0); }
		}
	}

	public void HandleReadyToStart(bool readyToStart) {
		if (!isLeader) { return; }

		startGameButton.interactable = readyToStart;
	}

	[Command]
	private void CmdSetDisplayName(string displayName) {
		DisplayName = displayName;
	}

	[Command]
	public void CmdReadyUp() {
		if (string.IsNullOrEmpty(Team)) { return; }

		IsReady = !IsReady;
		Room.NotifyPlayersOfReadyState();
	}

	[Command]
	public void CmdChooseTeam(string team) {
		if (Team == team) {
			IsReady = false;
			Room.NotifyPlayersOfReadyState();
			Team = string.Empty;
			return;
		}
		Team = team;
	}

	[Command]
	public void CmdStartGame() {
		if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

		Room.StartGame();
	}

	[Command]
	public void CmdDisconnect() {
		this.netIdentity.connectionToClient.Disconnect();
	}
}
