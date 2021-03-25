using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
	private static IDictionary<string, int> actionStates = new Dictionary<string, int>();

	private static Controls controls;
	public static Controls Controls
	{
		get
		{
			if (controls != null) { return controls; }
			return controls = new Controls();
		}
	}

	private void Awake() => SceneManager.sceneLoaded += OnSceneLoaded;

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		actionStates = new Dictionary<string, int>();
	}

	private void OnEnable() => Controls.Enable();
	private void OnDisable() => Controls.Disable();
	private void OnDestroy() => controls = null;

	public static void Add(string actionName) {
		actionStates.TryGetValue(actionName, out int value);
		actionStates[actionName] = value + 1;

		UpdateactionState(actionName);
	}

	public static void Remove(string actionName) {
		actionStates.TryGetValue(actionName, out int value);
		actionStates[actionName] = Mathf.Max(value - 1, 0);

		UpdateactionState(actionName);
	}

	private static void UpdateactionState(string actionName) {
		int value = actionStates[actionName];

		if (value > 0) {
			Controls.asset.FindAction(actionName).Disable();

			return;
		}

		Controls.asset.FindAction(actionName).Enable();
	}
}
