using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MovementEffects;

public class BattleGameScene : MonoBehaviour {

    public delegate void GameOverEvent();
    public event GameOverEvent OnGameOverEvent;

    public delegate void VictoryEvent();
    public event VictoryEvent OnVictoryEvent;

    public PlayerMain[] players { get; private set; }
    public PlayerMain mainPlayer { get; private set; }

    public string nextScene;

    public void RestartCurrentScene(){
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
        if (OnGameOverEvent != null) OnGameOverEvent();
        Timing.RunCoroutine(_PlayPlayersDeathScenario());
    }

    protected virtual IEnumerator<float> _PlayPlayersDeathScenario() {
        yield return Timing.WaitForSeconds(3f);
        RestartCurrentScene();
    }

    protected virtual void Victory()
    {
        if (OnVictoryEvent != null) OnVictoryEvent();
        Timing.RunCoroutine(_PlayVictoryScenario());
    }

    protected virtual IEnumerator<float> _PlayVictoryScenario()
    {
        yield return Timing.WaitForSeconds(3f);
        SceneManager.LoadScene(nextScene);
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

    protected virtual void OnDestroy()
    {
        mainPlayer.GetComponent<PlayerConstitution>().OnDieEvent -= OnMainPlayerDeath;
    }
}
