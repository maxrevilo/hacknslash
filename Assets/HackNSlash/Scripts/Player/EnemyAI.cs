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
    private BattleGameScene battleGameScene;

    [SerializeField]
	private float defAttackDelay = 1f;

	[SerializeField]
	private CollisionPub sightColliderPub;

    ActionList actions;

    protected override void Awake() {
        base.Awake();
        battleGameScene = GetComponentInParent<BattleGameScene>();
        actions = new ActionList();
    }

	protected override void Start () {
        base.Start();
		if(sightColliderPub == null) throw new Exception("sightColliderPub not found");
	}

    protected override void _Reset()
    {
        actions.Clear();
        actions.PushFront(new DetectEnemies(this, battleGameScene, sightColliderPub));
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

}
