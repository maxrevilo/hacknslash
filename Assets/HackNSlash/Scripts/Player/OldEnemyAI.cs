using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
[RequireComponent(typeof(PlayerStability))]
public class OldEnemyAI : Resetable {
	public float defAttackDistance = 3f; 
	
	[SerializeField]
	private float defAttackDelay = 1f;
	
	private CountDown attackDelay;

	private BattleGameScene battleGameScene;

	private PlayerMain target;
	
    private PlayerMain playerMain;
    private PlayerAttack playerAttack;
    private PlayerMotion playerMotion;
	private PlayerStability playerStability;

	[SerializeField]
	private CollisionPub sightColliderPub;

	private ArrayList enemiesInSight;

    protected override void Awake() {
        base.Awake();
		target = null;
		playerMain = GetComponent<PlayerMain>();
		playerAttack = GetComponent<PlayerAttack>();
		playerMotion = GetComponent<PlayerMotion>();
		playerStability = GetComponent<PlayerStability>();

		enemiesInSight = new ArrayList();

		sightColliderPub.OnTriggerEnterEvent += OnSight;
		sightColliderPub.OnTriggerExitEvent += OutOfSight;

		playerStability.OnStunLockedEvent += Interrupted;
		playerStability.OnKnockedBackEvent += Interrupted;
		playerStability.OnThrownEvent += Interrupted;
	}

	protected override void Start () {
        base.Start();
		battleGameScene = GetComponentInParent<BattleGameScene>();
		if(battleGameScene == null) throw new Exception("battleGameScene not found");

		if(sightColliderPub == null) throw new Exception("sightColliderPub not found");
	}


    protected override void _Reset()
    {
        enemiesInSight.Clear();
        attackDelay.Stop();
        target = null;
    }

    void Interrupted() {
        attackDelay.Restart(defAttackDelay);
    }

	void OnSight(Collider other) {
		PlayerMain player = other.GetComponent<PlayerMain>();

		if(player == null) return;

		if(isAlly(player)) {

		} else {
			if(target == null) {
				// TODO: T1. This should be a procedure to look on the
				// enemiesInSight list for the best target to lock on
				LockTarget(player);
			}
			enemiesInSight.Add(player);
		}
	}

	void OutOfSight(Collider other) {
		PlayerMain player = other.GetComponent<PlayerMain>();
		if(player == null) return;

		if(isAlly(player)) {

		} else {
			if(player == target) {
				// TODO: T2. The new version of LockTarget (as mentioned in T1)
				// should run here to look at for other enemies on sight.
				Disengage();
			}
			enemiesInSight.Remove(player);
		}
	}

	void LockTarget(PlayerMain player) {
		target = player;
		attackDelay.Restart(defAttackDelay);
	}

	void Disengage() {
		target = null;
	}

	bool isAlly(PlayerMain player) { return player.team == playerMain.team;}

    protected override void Update () {
        base.Update();
		if(target != null) {
			Debug.DrawLine(transform.position, target.transform.position, Color.red);
		}
	}

    protected override void FixedUpdate () {
        base.FixedUpdate();
		if(target != null) {
			playerMotion.LookAt(target.transform.position);

			float distance = Vector3.Distance(target.transform.position, transform.position);
			if(distance >= defAttackDistance) {
				playerMotion.Advance();
			} else {
				playerMotion.Stop();
				
				if(attackDelay.HasFinished()) {
					playerAttack.Attack(target.transform.position);
					attackDelay.Restart(defAttackDelay);
				}
			}
		} else {
			playerMotion.Stop();
		}
	}
}
