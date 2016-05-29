using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour {
	public int team = 1;

    PlayerWeaponDef playerWeaponDef;

    void Awake() {
        if (hitAreaSpawnZone == null) throw new Exception("hitAreaSpawnZone not set");
        playerWeaponDef = GetComponent<PlayerWeaponDef>();
    }

	void Start () {
	    #region Atack
		currentAttackCooldown = 0;
        #endregion
        #region Motion
        playerRigidBody = GetComponent<Rigidbody>();
        #endregion
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void FixedUpdate () {
	    #region Atack
		currentAttackCooldown -= Time.fixedDeltaTime;
	    #endregion
	    #region Motion
		if(advancing) {
			playerRigidBody.MovePosition(
				transform.position + defSpeed * transform.forward
			);
		}
	    #endregion
	}

    #region Motion
	[SerializeField]
	private float defSpeed = 0.3f;
	private bool advancing;
	private Rigidbody playerRigidBody;

	public void Advance() {
		advancing = true;
	}
	public void Stop() {
		advancing = false;
	}
	
	public void LookAt(Vector3 position) {
		Vector3 forward = position - transform.position;
		forward.y = 0;
		playerRigidBody.MoveRotation(
			Quaternion.LookRotation(forward, transform.up)
		);
	}
    #endregion

    #region Atack
	public Transform hitAreaSpawnZone;

	[SerializeField]
	private float defAttackCooldown = 1;

    private float currentAttackCooldown;

    public void Attack(Vector3 position) {
		if(currentAttackCooldown <= 0) {
			//Debug.DrawLine(transform.position, position, Color.red, 0.05f);
			currentAttackCooldown = defAttackCooldown;
			LookAt(position);
            StartCoroutine(ActivateAtackArea());
        }
	}

	private IEnumerator ActivateAtackArea() {
        yield return new WaitForFixedUpdate();
        GameObject hitAreaGO = PoolingSystem.Instance.InstantiateAPS(
            "HitArea",
            hitAreaSpawnZone.position,
            hitAreaSpawnZone.rotation,
            transform.parent.gameObject
        );
        HitArea hitArea = hitAreaGO.GetComponent<HitArea>();
        hitArea.spawner = this;
        hitArea.playerWeaponDef = playerWeaponDef;
    }
    #endregion
}
