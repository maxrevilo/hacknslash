using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMotion))]
[RequireComponent(typeof(PlayerStability))]
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
    private PlayerStability playerStability;
    private SoftCollision softCollider;

    private CountDown meleeAttackCooldown;
    private CountDown meleeAttackRestitution;
    private CountDown dashCooldown;
    private CountDown dashRestitution;
    private CountDown chargedAttackCooldown;
    private CountDown chargedAttackRestitution;
    private CountDown chargedAttackChargeCountDown;
    private bool isChargingAttack;

    private Rigidbody playerRigidBody;
    
	private int dashingLayer;
    private int playerLayer;
    

	void Awake() {
        playerMain = GetComponent<PlayerMain>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerMotion = GetComponent<PlayerMotion>();
        playerStability = GetComponent<PlayerStability>();
        softCollider = GetComponentInChildren<SoftCollision>();

        if (hitAreaSpawnZone == null) throw new Exception("hitAreaSpawnZone not set");

        playerLayer = gameObject.layer;
        dashingLayer = LayerMask.NameToLayer("Dashing");

        playerStability.OnStunLockedEvent += Interrupted;
		playerStability.OnKnockedBackEvent += Interrupted;
		playerStability.OnThrownEvent += Interrupted;
    }

    void Start () {
        meleeAttackCooldown.Stop();
        meleeAttackRestitution.Stop();
        chargedAttackCooldown.Stop();
        chargedAttackRestitution.Stop();
        dashCooldown.Stop();
        dashRestitution.Stop();
        chargedAttackChargeCountDown.Stop();
        isChargingAttack = false;
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
  
            GenerateDashHitArea(dashSpeed * dashDeltaTime);
        }
	}

    void Interrupted() {
        FinishHeavyAttackCharge(false);

        // Noting more to interrupt, the animation is in charge of that part of the logic (which is bad).
    }

    public void Attack(Vector3 position) {
        playerMotion.Stop();
        if (IsAbleToAttack() && meleeAttackCooldown.HasFinished()) {
            Debug.DrawLine(transform.position, position, Color.red, 1f);
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

    public bool IsAtacking()
    {
        return !meleeAttackRestitution.HasFinished()
            || !dashRestitution.HasFinished()
            || !chargedAttackRestitution.HasFinished();
    }

    public bool IsAbleToAttack()
    {
        //return playerStability.IsStable() && !IsAtacking();
        return !IsAtacking();
    }

    public void Dash(Vector3 direction) {
        playerMotion.Stop();
        if (IsAbleToAttack()) {
            Debug.DrawLine(transform.position, transform.position + direction * dashWeaponDef.dashDistance, Color.green, 1.5f);
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
        softCollider.enabled = false;
    }
    
    private IEnumerator FinishDashing() {
        yield return new WaitForFixedUpdate();
        gameObject.layer = playerLayer;
        playerRigidBody.velocity = Vector3.zero;
        softCollider.enabled = true;
    }

    public void ChargeHeavyAttack()
    {
        playerMotion.Stop();
        if(IsAbleToAttack() && chargedAttackCooldown.HasFinished() && !isChargingAttack)
        {
            chargedAttackChargeCountDown.Restart(chargedWeaponDef.chargeTime);
            isChargingAttack = true;

            if (OnChargingHeavyAttackEvent != null) OnChargingHeavyAttackEvent(playerMain);
        }
    }

    public void ReleaseHeavyAttack()
    {
        bool isCharged = isChargingAttack && chargedAttackChargeCountDown.HasFinished();
        FinishHeavyAttackCharge(isCharged);
    }

    private void FinishHeavyAttackCharge(bool successful)
    {
        chargedAttackChargeCountDown.Stop();
        isChargingAttack = false;
        if (successful)
        {
            chargedAttackCooldown.Restart(chargedWeaponDef.attackCooldown);
            chargedAttackRestitution.Restart(chargedWeaponDef.attackRestitution);
        }
        if (OnReleaseHeavyAttackEvent != null) OnReleaseHeavyAttackEvent(playerMain, successful);
    }

    internal IEnumerator ActivateChargedAttackArea()
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
    
    private void GenerateDashHitArea(float distance)
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
}
