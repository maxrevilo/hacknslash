using System;
using UnityEngine;

public class BattleGameScene : MonoBehaviour {
    
    public PlayerMain[] players { get; private set; }

	void Awake() {
		Application.targetFrameRate = 120;
		players = GetComponentsInChildren<PlayerMain>(true);
	}

	void Start () {
	}
	
	void FixedUpdate () {
	}
}
