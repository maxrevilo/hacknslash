using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerMotion))]
public class EnemyAI : MonoBehaviour {
	public float defAttackDistance = 1.3f; 
	
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
			attackDelay -= Time.fixedDeltaTime;

			float distance = Vector3.Distance(target.transform.position, transform.position);
			if(distance >= defAttackDistance) {
				playerMotion.LookAt(target.transform.position);
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
