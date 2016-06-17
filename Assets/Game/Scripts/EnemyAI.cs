using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
public class EnemyAI : MonoBehaviour {
	public float defAttackDistance = 3f; 
	
	[SerializeField]
	private float defAttackDelay = 1f;
	
	private float attackDelay = 0;

	private BattleGameScene battleGameScene;

	private PlayerMain target;
	
    private PlayerMain playerMain;
    private PlayerAttack playerAttack;
    private PlayerMotion playerMotion;

	[SerializeField]
	private CollisionPub sightColliderPub;

	private ArrayList enemiesInSight;

	void Awake() {
		target = null;
		playerMain = GetComponent<PlayerMain>();
		playerAttack = GetComponent<PlayerAttack>();
		playerMotion = GetComponent<PlayerMotion>();

		enemiesInSight = new ArrayList();
	}

	void Start () {
		battleGameScene = (BattleGameScene) GetComponentInParent(typeof(BattleGameScene));
		if(battleGameScene == null) throw new Exception("battleGameScene not found");

		if(sightColliderPub == null) throw new Exception("sightColliderPub not found");
		sightColliderPub.OnTriggerEnterEvent += OnSight;
		sightColliderPub.OnTriggerExitEvent += OutOfSight;
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
		attackDelay = defAttackDelay;
	}

	void Disengage() {
		target = null;
	}

	bool isAlly(PlayerMain player) { return player.team == playerMain.team;}

	void Update () {
		if(target != null) {
			Debug.DrawLine(transform.position, target.transform.position, Color.red);
		}
	}

	void FixedUpdate () {
		if(target != null) {
			playerMotion.LookAt(target.transform.position);

			attackDelay -= Time.fixedDeltaTime;

			float distance = Vector3.Distance(target.transform.position, transform.position);
			if(distance >= defAttackDistance) {
				playerMotion.Advance();
			} else {
				playerMotion.Stop();
				
				if(attackDelay <= 0) {
					playerAttack.Attack(target.transform.position);
					attackDelay = defAttackDelay;
				}
			}
		} else {
			playerMotion.Stop();
		}
	}
}
