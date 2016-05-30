using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class EnemyAI : MonoBehaviour {
	private PlayerMain playerMain;
	
	public float defAttackDistance = 1.3f; 
	
	[SerializeField]
	private float defAttackDelay = 1f;
	
	private float attackDelay = 0;

	private BattleGameScene battleGameScene;

	private PlayerMain target;

	void Awake() {
		target = null;
		playerMain = GetComponent<PlayerMain>();
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
				playerMain.LookAt(target.transform.position);
				playerMain.Advance();
			} else {
				playerMain.Stop();
				
				if(attackDelay <= 0) {
					playerMain.Attack(target.transform.position);
					attackDelay = defAttackDelay;
				}
			}
		}
	}
}
