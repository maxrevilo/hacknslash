using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMotion))]
public class PlayerAttack : MonoBehaviour {

    public delegate void ChargingHeavyAttackEvent(PlayerMain playerMain);
    public event ChargingHeavyAttackEvent OnChargingHeavyAttackEvent;
    
    public delegate void ReleaseHeavyAttackEvent(PlayerMain playerMain, bool fullyCharged);
    public event ReleaseHeavyAttackEvent OnReleaseHeavyAttackEvent;

    public delegate void AttackingEvent(PlayerMain playerMain, PlayerWeaponDef weapon);
    public event AttackingEvent OnAttackingEvent;

    public delegate void DashingEvent(PlayerMain playerMain, PlayerWeaponDef weapon);
    public event DashingEvent OnDashingEvent;

    public Transform hitAreaSpawnZone;

    public PlayerWeaponDef meleeWeaponDef;
    public PlayerWeaponDef chargedWeaponDef;
    public PlayerWeaponDef dashWeaponDef;

    private PlayerMain playerMain;
    private PlayerMotion playerMotion;

    private CountDown meleeAttackCooldown;
    private CountDown meleeAttackRestitution;
    private CountDown dashCooldown;
    private CountDown dashRestitution;
    private CountDown chargedAttackCooldown;
    private CountDown chargedAttackRestitution;

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

        meleeAttackCooldown = new CountDown();
        meleeAttackRestitution = new CountDown();
        chargedAttackCooldown = new CountDown();
        chargedAttackRestitution = new CountDown();
        dashCooldown = new CountDown();
        dashRestitution = new CountDown();
    }

	void Start () {
        meleeAttackCooldown.Stop();
        meleeAttackRestitution.Stop();
        chargedAttackCooldown.Stop();
        chargedAttackRestitution.Stop();
        dashCooldown.Stop();
        dashRestitution.Stop();
    }

	void FixedUpdate () {
        float fixedDeltaTime = Time.fixedDeltaTime;
        
        if (!dashRestitution.HasFinished()) {
            float dashDeltaTime = fixedDeltaTime;
            bool isFinishingDash = dashRestitution.TimeToFinish() <= 1.5f * fixedDeltaTime;
            if (isFinishingDash) {
                StartCoroutine(FinishDashing());
                dashDeltaTime = dashRestitution.TimeToFinish();
                dashRestitution.Stop();
            }

            float dashSpeed = dashWeaponDef.dashDistance / dashWeaponDef.attackRestitution;
            playerRigidBody.velocity = transform.forward * dashSpeed;
  
            generateDashHitArea(dashSpeed * dashDeltaTime);
        }
	}

    public void Attack(Vector3 position) {
        if (!isAtacking() && meleeAttackCooldown.HasFinished()) {
            Debug.DrawLine(transform.position, position, Color.red, 1f);
            playerMotion.Stop();
            meleeAttackCooldown.Restart(meleeWeaponDef.attackCooldown);
            meleeAttackRestitution.Restart(meleeWeaponDef.attackRestitution);
            playerMotion.LookAt(position, true);
            if (OnAttackingEvent != null) OnAttackingEvent(playerMain, meleeWeaponDef);
        }
    }

    public void ActivateAtackArea()
    {
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
        return !meleeAttackRestitution.HasFinished()
            || !dashRestitution.HasFinished()
            || !chargedAttackRestitution.HasFinished();
    }

    public void Dash(Vector3 direction) {
        if (!isAtacking()) {
            Debug.DrawLine(transform.position, transform.position + direction * dashWeaponDef.dashDistance, Color.green, 1.5f);
            playerMotion.Stop();
            playerMotion.LookTowards(direction, true, true);
            StartCoroutine(ActivateDashMode());
            if (OnDashingEvent != null) OnDashingEvent(playerMain, meleeWeaponDef);
        }
    }
    
    private IEnumerator ActivateDashMode() {
        yield return new WaitForEndOfFrame();
        dashCooldown.Restart(dashWeaponDef.attackCooldown);
        dashRestitution.Restart(dashWeaponDef.attackRestitution);
        gameObject.layer = dashingLayer;
    }
    
    private IEnumerator FinishDashing() {
        yield return new WaitForFixedUpdate();
        gameObject.layer = playerLayer;
        playerRigidBody.velocity = Vector3.zero;
    }

    public void ChargeHeavyAttack()
    {
        if(!isAtacking() && chargedAttackCooldown.HasFinished() && !isChargingHeavyAttack())
        {
            chargingStartedAt = Time.time;
            playerMotion.Stop();
            
            if(OnChargingHeavyAttackEvent != null) OnChargingHeavyAttackEvent(playerMain);
        }
    }

    public void ReleaseHeavyAttack()
    {
        float chargeTime = Time.time - chargingStartedAt;
        bool isCharged = chargingStartedAt != 0 && chargeTime >= chargedWeaponDef.chargeTime;
        chargingStartedAt = 0;
        if (isCharged)
        {
            chargedAttackCooldown.Restart(chargedWeaponDef.attackCooldown);
            chargedAttackRestitution.Restart(chargedWeaponDef.attackRestitution);

            StartCoroutine(ActivateChargedAttackArea());
        }
        
        if(OnReleaseHeavyAttackEvent != null) OnReleaseHeavyAttackEvent(playerMain, isCharged);
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
