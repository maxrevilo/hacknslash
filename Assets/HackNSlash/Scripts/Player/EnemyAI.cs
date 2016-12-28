using System;
using DarkWinter.Util.DataStructures;
using HackNSlash.Player.AIActions;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
[RequireComponent(typeof(PlayerStability))]
public class EnemyAI : Resetable {
	public float defAttackDistance = 3f;

    public bool debug = false;
    private BattleGameScene battleGameScene;

    [SerializeField]
	public float defAttackDelay = 1f;

	[SerializeField]
	private CollisionPub sightColliderPub;

    protected ActionList actions;

    protected override void Awake() {
        base.Awake();
        actions = new ActionList();
    }

	protected override void Start () {
        battleGameScene = GetComponentInParent<BattleGameScene>();
		if(sightColliderPub == null) throw new Exception("sightColliderPub not found");

        base.Start();
	}

    protected override void _Reset()
    {
        actions.Clear();
        actions.PushFront(
            DetectEnemies.Create().Initialize(this, battleGameScene, sightColliderPub)
        );
    }

    void Interrupted() {
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        actions.Update(Time.fixedDeltaTime);
    }

    protected override void Update () {
        base.Update();
	}

    void OnGUI()
    {
        if(debug) {
            string txt = "actions:\n";
            int lines = 1;
            foreach(LAction action in actions.list) {
                string actionName = action.GetType().Name;
                string color = action.isBlocked ? "#800000ff" : "#00ff00ff";
                txt += String.Format("<color={2}>{0} - {1}</color>\n", actionName, action.lanes, color); 
                lines++;
            }

            Vector3 position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position + Vector3.up * 2f);
            GUI.Box(new Rect(position.x, Screen.height - position.y, 150, 15 * lines + 10), txt);
        }
    }

}
