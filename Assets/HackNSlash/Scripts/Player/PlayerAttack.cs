using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMotion))]
[RequireComponent(typeof(PlayerStability))]
public class PlayerAttack : Resetable {

    public delegate void ChargingHeavyAttackEvent(PlayerMain playerMain);
    public event ChargingHeavyAttackEvent OnChargingHeavyAttackEvent;
    
    public delegate void ReleaseHeavyAttackEvent(PlayerMain playerMain, bool successful);
    public event ReleaseHeavyAttackEvent OnReleaseHeavyAttackEvent;

    public delegate void ForcedHeavyAttackEvent(PlayerMain playerMain);
    public event ForcedHeavyAttackEvent OnForcedHeavyAttackEvent;

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
    private HitArea dashHitArea;
    private bool isDashing;
    

	protected override void Awake() {
        base.Awake();
        playerMain = GetComponent<PlayerMain>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerMotion = GetComponent<PlayerMotion>();
        playerStability = GetComponent<PlayerStability>();
        softCollider = GetComponentInChildren<SoftCollision>();

        if (hitAreaSpawnZone == null) throw new Exception("hitAreaSpawnZone not set");

        dashingLayer = LayerMask.NameToLayer("Dashing");

        playerStability.OnStunLockedEvent += Interrupted;
        playerStability.OnKnockedBackEvent += Interrupted;
        playerStability.OnThrownEvent += Interrupted;
    }

    protected override void Start () {
        base.Start();
    }

    protected override void OnDestroy() {
        playerStability.OnStunLockedEvent -= Interrupted;
        playerStability.OnKnockedBackEvent -= Interrupted;
        playerStability.OnThrownEvent -= Interrupted;
    }

    protected override void _Reset()
    {
        playerLayer = gameObject.layer;

        meleeAttackCooldown.Stop();
        meleeAttackRestitution.Stop();

        chargedAttackCooldown.Stop();
        chargedAttackRestitution.Stop();
        chargedAttackChargeCountDown.Stop();
        isChargingAttack = false;
        isDashing = false;

        dashCooldown.Stop();
        dashRestitution.Stop();
        DestroyDashHitArea();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        #region Dash
        float fixedDeltaTime = Time.fixedDeltaTime;

        if (isDashing)
        {
            if (dashRestitution.HasFinished())
            {
                Timing.RunCoroutine(FinishDashing(), Segment.FixedUpdate);
            } else {
                float dashDeltaTime = fixedDeltaTime;
                bool isFinishingDash = dashRestitution.TimeToFinish() <= fixedDeltaTime;
                if (isFinishingDash)
                {
                    dashDeltaTime = dashRestitution.TimeToFinish();
                    dashRestitution.Stop();
                }

                float frac = dashDeltaTime / fixedDeltaTime;

                float dashSpeed = dashWeaponDef.dashDistance / dashWeaponDef.attackRestitution;
                playerRigidBody.velocity = transform.forward * dashSpeed * frac;

                UpdateDashHitArea(dashSpeed * dashDeltaTime);
            }        
            #endregion Dash
        }
    }

    void Interrupted() {
        FinishHeavyAttackCharge(false);

        // Noting more to interrupt, the animation is in charge of that part of the logic (which is bad).
    }

    public bool IsAtacking()
    {
        return !meleeAttackRestitution.HasFinished()
            || !dashRestitution.HasFinished()
            || !chargedAttackRestitution.HasFinished();
    }

    public bool IsAbleToAttack()
    {
        return playerStability.IsStable() && !IsAtacking();
    }

    #region BasicAttack
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
            hitAreaSpawnZone.transform.gameObject
        );

        HitArea hitArea = hitAreaGO.GetComponent<HitArea>();
        hitArea.spawner = playerMain;
        hitArea.playerWeaponDef = meleeWeaponDef;

        hitArea.ResetComponent();
        hitAreaGO.transform.SetParent(transform.parent);
    }
    #endregion BasicAttack

    #region Dash
    public void Dash(Vector3 direction) {
        playerMotion.Stop();
        if (IsAbleToAttack()) {
            Debug.DrawLine(transform.position, transform.position + direction * dashWeaponDef.dashDistance, Color.green, 3f);
            playerMotion.LookTowards(direction, true, true);

            dashCooldown.Restart(dashWeaponDef.attackCooldown);
            dashRestitution.Restart(dashWeaponDef.attackRestitution);

            gameObject.layer = dashingLayer;
            softCollider.enabled = false;
            isDashing = true;

            DestroyDashHitArea();
            GameObject hitAreaGO = PoolingSystem.Instance.InstantiateAPS(
                "HitAreaDash",
                transform.position,
                transform.rotation,
                transform.parent.gameObject
            );
            dashHitArea = hitAreaGO.GetComponent<HitArea>();
            dashHitArea.spawner = playerMain;
            dashHitArea.playerWeaponDef = dashWeaponDef;
            dashHitArea.ResetComponent();

            if (OnDashingEvent != null) OnDashingEvent(playerMain, meleeWeaponDef);
        }
    }

    private void UpdateDashHitArea(float distance)
    {
        dashHitArea.transform.rotation = transform.rotation;
        dashHitArea.transform.position = transform.position + dashHitArea.transform.forward * distance * 0.5f;

        Vector3 scale = dashHitArea.transform.localScale;
        scale.z = distance;
        dashHitArea.transform.localScale = scale;
    }

    private bool DestroyDashHitArea()
    {
        if (dashHitArea != null)
        {
            dashHitArea.gameObject.DestroyAPS();
            dashHitArea = null;
            return true;
        }
        else return false;
    }

    private IEnumerator<float> FinishDashing() {
        gameObject.layer = playerLayer;
        isDashing = false;
        playerRigidBody.velocity = Vector3.zero;
        softCollider.enabled = true;
        DestroyDashHitArea();
        yield return 0f;
    }
    #endregion Dash

    #region HeavyAttack
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

    public void ForceReleaseHeavyAttack()
    {
        chargedAttackChargeCountDown.Stop();
        chargedAttackCooldown.Restart(chargedWeaponDef.attackCooldown);
        chargedAttackRestitution.Restart(chargedWeaponDef.attackRestitution);
        if (OnForcedHeavyAttackEvent != null) OnForcedHeavyAttackEvent(playerMain);
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
    #endregion HeavyAttack

}
