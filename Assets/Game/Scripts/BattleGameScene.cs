using System;
using UnityEngine;

public class BattleGameScene : MonoBehaviour {
    
    public PlayerMain[] players { get; private set; }

	void Awake() {
		Application.targetFrameRate = 120;
		players = GetComponentsInChildren<PlayerMain>(true);
	}

	void Start () {
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = true;
		Screen.orientation = ScreenOrientation.AutoRotation;
	}
	
	void FixedUpdate () {
	}
}
