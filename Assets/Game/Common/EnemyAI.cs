using System;
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

	void Awake() {
		target = null;
		playerMain = GetComponent<PlayerMain>();
		playerAttack = GetComponent<PlayerAttack>();
		playerMotion = GetComponent<PlayerMotion>();
	}

	void Start () {
		battleGameScene = (BattleGameScene) GetComponentInParent(typeof(BattleGameScene));
		if(battleGameScene == null) throw new Exception("BattleGameScene not found");
	}

	void Update () {
		if(target != null) {
			Debug.DrawLine(transform.position, target.transform.position, Color.red);
		}
	}

	void FixedUpdate () {
		if(target == null) {
			foreach (PlayerMain player in battleGameScene.players) {
				if(player.team != playerMain.team) {
					target = player;
					attackDelay = defAttackDelay;
					break;
				}
			}
		} else {
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
		}
	}
}
