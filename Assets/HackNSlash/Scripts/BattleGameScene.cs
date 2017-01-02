using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MovementEffects;

public class BattleGameScene : MonoBehaviour {
    
    public PlayerMain[] players { get; private set; }
    public PlayerMain mainPlayer { get; private set; }

    public void restartCurrentScene(){
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
     }

    protected virtual void Awake() {
		Application.targetFrameRate = 120;
		players = GetComponentsInChildren<PlayerMain>(true);
        mainPlayer = Array.Find<PlayerMain>(players, p => p.CompareTag("Player"));
        if (mainPlayer == null) throw new Exception("Main player not found");

        mainPlayer.GetComponent<PlayerConstitution>().OnDieEvent += OnMainPlayerDeath;
    }

    protected virtual void OnMainPlayerDeath(PlayerMain playerMain, float lastHit) {
        mainPlayer.GetComponent<PlayerConstitution>().OnDieEvent -= OnMainPlayerDeath;
        Timing.RunCoroutine(_PlayPlayersDeathScenario());
    }

    protected virtual IEnumerator<float> _PlayPlayersDeathScenario() {
        yield return Timing.WaitForSeconds(3f);
        restartCurrentScene();
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
