using System;
using UnityEngine;

public class BattleGameScene : MonoBehaviour {
    
    public PlayerMain[] players { get; private set; }
    public PlayerMain mainPlayer { get; private set; }

    protected virtual void Awake() {
		Application.targetFrameRate = 120;
		players = GetComponentsInChildren<PlayerMain>(true);
        mainPlayer = Array.Find<PlayerMain>(players, p => p.CompareTag("Player"));
        if (mainPlayer == null) Debug.LogWarning("Main player not found");
    }

    protected virtual void Start () {
		Screen.autorotateToPortrait = true;
		Screen.autorotateToPortraitUpsideDown = true;
		Screen.orientation = ScreenOrientation.AutoRotation;
	}

    protected virtual void Update()
    {
    }

    protected virtual void FixedUpdate () {
    }

}
