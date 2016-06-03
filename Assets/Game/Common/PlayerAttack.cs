using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMotion))]
public class PlayerAttack : MonoBehaviour {

    public bool useVelocityForDash = false;
    
    public Transform hitAreaSpawnZone;

    public PlayerWeaponDef meleeWeaponDef;
    public PlayerWeaponDef chargedWeaponDef;
    public PlayerWeaponDef dashWeaponDef;

    private PlayerMain playerMain;
    private PlayerMotion playerMotion;

    private float meleeAttackCooldown;
    private float meleeAttackRestitution;

    private float dashCooldown;
    private float dashRestitution;

    private float chargedAttackCooldown;
    private float chargedAttackRestitution;

    private Rigidbody playerRigidBody;

    private float chargingStartedAt = 0;
    
	private int dashingLayer;
    private int playerLayer;
    

	void Awake() {
        playerMain = GetComponent<PlayerMain>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerMotion = GetComponent<PlayerMotion>();

		if (hitAreaSpawnZone == null) throw new Exception("hitAreaSpawnZone not set");

        playerLayer = gameObject.layer;
        dashingLayer = LayerMask.NameToLayer("Dashing");
	}

	void Start () {
		meleeAttackCooldown = 0;
        meleeAttackRestitution = 0;
        chargedAttackCooldown = 0;
        chargedAttackRestitution = 0;
        dashCooldown = 0;
        dashRestitution = 0;
	}
	
	void FixedUpdate () {
		//TODO: This shouldn't be integral but use timestamp diffs.
        float fixedDeltaTime = Time.fixedDeltaTime;
        meleeAttackCooldown -= fixedDeltaTime;
        meleeAttackRestitution -= fixedDeltaTime;
        chargedAttackCooldown -= fixedDeltaTime;
        chargedAttackRestitution -= fixedDeltaTime;
        
        if (dashRestitution > 0) {
            float dashDeltaTime = fixedDeltaTime;
            if(dashRestitution - fixedDeltaTime <= 0) {
                dashRestitution = 0;
                StartCoroutine(FinishDashing());
                dashDeltaTime = dashRestitution;
            }

            float dashSpeed = dashWeaponDef.dashDistance / dashWeaponDef.attackRestitution;
            if(useVelocityForDash) {
                playerRigidBody.velocity = transform.forward * dashSpeed;
            } else {
                playerRigidBody.MovePosition(
                    transform.position + dashSpeed * dashDeltaTime * transform.forward
                );
            }
            dashRestitution -= fixedDeltaTime;
            
            generateDashHitArea(dashSpeed * dashDeltaTime);
        }
        dashCooldown -= fixedDeltaTime;
	}

    public void Attack(Vector3 position) {
        if (!isAtacking() && meleeAttackCooldown <= 0) {
            Debug.DrawLine(transform.position, position, Color.red, 1f);
            playerMotion.Stop();
            meleeAttackCooldown = meleeWeaponDef.attackCooldown;
            meleeAttackRestitution = meleeWeaponDef.attackRestitution;
            playerMotion.LookAt(position, true);
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
        hitArea.spawner = playerMain;
        hitArea.playerWeaponDef = meleeWeaponDef;
    }

    public bool isAtacking()
    {
        return meleeAttackRestitution > 0
            || dashRestitution > 0
            || chargedAttackRestitution > 0;
    }

    public void Dash(Vector3 direction) {
        if (!isAtacking()) {
            Debug.DrawLine(transform.position, transform.position + direction * dashWeaponDef.dashDistance, Color.green, 1.5f);
            playerMotion.Stop();
            playerMotion.LookTowards(direction, true);
            StartCoroutine(ActivateDashMode());
        }
    }
    
    private IEnumerator ActivateDashMode() {
        yield return new WaitForFixedUpdate();
        dashCooldown = dashWeaponDef.attackCooldown;
        dashRestitution = dashWeaponDef.attackRestitution;
        if(!useVelocityForDash) {
            dashRestitution += Time.fixedDeltaTime/2;
        }
        gameObject.layer = dashingLayer;
    }
    
    private IEnumerator FinishDashing() {
        yield return new WaitForFixedUpdate();
        gameObject.layer = playerLayer;
        if(useVelocityForDash) {
            playerRigidBody.velocity = Vector3.zero;
        }
    }

    public void ChargeHeavyAttack()
    {
        if(!isAtacking() && chargedAttackCooldown <= 0 && !isChargingHeavyAttack())
        {
            chargingStartedAt = Time.time;
            playerMotion.Stop();
        }
    }

    public void ReleaseHeavyAttack()
    {
        float chargeTime = Time.time - chargingStartedAt;
        bool isCharged = chargingStartedAt != 0 && chargeTime >= chargedWeaponDef.chargeTime;
        chargingStartedAt = 0;
        if (isCharged)
        {
            chargedAttackCooldown = chargedWeaponDef.attackCooldown;
            chargedAttackRestitution = chargedWeaponDef.attackRestitution;

            StartCoroutine(ActivateChargedAttackArea());
        }
    }

    private IEnumerator ActivateChargedAttackArea()
    {
        yield return new WaitForFixedUpdate();
        GameObject hitAreaGO = PoolingSystem.Instance.InstantiateAPS(
            "HitAreaChargedWave",
            transform.position,
            transform.rotation,
            transform.parent.gameObject
        );
        HitArea hitArea = hitAreaGO.GetComponent<HitArea>();
        hitArea.spawner = playerMain;
        hitArea.playerWeaponDef = chargedWeaponDef;
    }
    
    private void generateDashHitArea(float distance)
    {
        GameObject hitAreaGO = PoolingSystem.Instance.InstantiateAPS(
            "HitAreaDash",
            transform.position,
            transform.rotation,
            transform.parent.gameObject
        );
        HitArea hitArea = hitAreaGO.GetComponent<HitArea>();
        hitArea.spawner = playerMain;
        hitArea.playerWeaponDef = dashWeaponDef;
        
        Vector3 scale = hitArea.transform.localScale;
        scale.z = distance;
        hitArea.transform.localScale = scale;

        Vector3 position = hitArea.transform.position;
        position += hitArea.transform.forward * distance * 0.5f;
        hitArea.transform.position = position;
    }

    public bool isChargingHeavyAttack()
    {  return chargingStartedAt != 0; }
}
