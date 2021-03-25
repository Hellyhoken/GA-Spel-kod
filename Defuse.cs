using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defuse : NetworkBehaviour
{
	private bool isDefuse = false;
	private float start = 0f;

	[SerializeField] public float defuseTime = 10f;

	private Coroutine defusing;

	private GameObject bomb = null;
	private GameObject Bomb {
		get {
			if (bomb == null) { return bomb = GameObject.Find("Bomb Planted(Clone)"); }
			return bomb;
		}
	}

	private RoundSystem roundSystem;
	public RoundSystem RoundSystem
	{
		get
		{
			if (roundSystem != null) { return roundSystem; }
			return roundSystem = GameObject.Find("RoundSystem(Clone)").GetComponent<RoundSystem>();
		}
	}

	private UIDefuseManager defuseManager;
	public UIDefuseManager DefuseManager
	{
		get
		{
			if (defuseManager != null) { return defuseManager; }
			return defuseManager = GameObject.Find("Canvas_PublicUI(Clone)").GetComponent<UIDefuseManager>();
		}
	}

	public override void OnStartAuthority()
	{
		if (transform.GetComponent<PlayerID>().isOrange) { return; }
		enabled = true;
		InputManager.Controls.Player.Use.performed += ctx => StartDefuse();
		InputManager.Controls.Player.Use.canceled += ctx => DefuseReleased();
	}

	private void Update()
	{
		if (isDefuse) {
			DefuseManager.Defusing((Time.time - start) / defuseTime);
		}
		if (isDefuse && (Vector3.Distance(bomb.transform.position, transform.position) > 1.5f || Vector3.SignedAngle(transform.forward, bomb.transform.position - transform.position, Vector3.up) > 45f)) {
			StopDefuse();
		}
	}

	private void StartDefuse() {
		if (Vector3.Distance(Bomb.transform.position, transform.position) > 1.5f || Vector3.SignedAngle(transform.forward, Bomb.transform.position - transform.position, Vector3.up) > 45f) { return; }
		isDefuse = true;
		defusing = StartCoroutine(Defusing());
		start = Time.time;

		foreach (string enable in ActionNames.Defuse)
		{
			InputManager.Add(enable);
		}
	}

	private IEnumerator Defusing() {
		yield return new WaitForSeconds(defuseTime);
		DefuseDone();
	}

	private void DefuseDone() {
		RoundSystem.DefuseBomb();
		Bomb.GetComponent<BombPlanted>().defused = true;
		StopDefuse();
	}

	private void StopDefuse() {
		StopCoroutine(defusing);
		isDefuse = false;
		DefuseManager.StopDefusing();

		foreach (string enable in ActionNames.Defuse)
		{
			InputManager.Remove(enable);
		}
	}

	private void DefuseReleased() {
		if (isDefuse) { StopDefuse(); }
	}
}
