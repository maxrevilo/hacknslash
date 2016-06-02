using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour {
    void Awake()
    {
        #region Atack
        if (hitAreaSpawnZone == null) throw new Exception("hitAreaSpawnZone not set");
        playerLayer = gameObject.layer;
        dashingLayer = LayerMask.NameToLayer("Dashing");
        #endregion
        #region Health
        lifeBar = GetComponentInChildren<LifeBar>();
        #endregion
    }

    void Start() {
        #region Atack
        meleeAttackCooldown = 0;
        meleeAttackRestitution = 0;
        chargedAttackCooldown = 0;
        chargedAttackRestitution = 0;
        dashCooldown = 0;
        dashRestitution = 0;
        #endregion
        #region Motion
        playerRigidBody = GetComponent<Rigidbody>();
        #endregion
        #region Health
        hitPoints = defHitPoints;
        #endregion
    }

    // Update is called once per frame
    void Update() {
    }

    void FixedUpdate() {
        #region Atack
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
                Debug.LogFormat("DashDeltaTime {0}/{1}", dashDeltaTime, fixedDeltaTime);
            }

            float dashSpeed = dashWeaponDef.dashDistance / dashWeaponDef.attackRestitution;
            playerRigidBody.MovePosition(
                transform.position + dashSpeed * dashDeltaTime * transform.forward
            );
            
            dashRestitution -= fixedDeltaTime;
        }
        dashCooldown -= fixedDeltaTime;
        #endregion
        #region Motion
        if (advancing) {
            playerRigidBody.MovePosition(
                transform.position + defSpeed * Time.fixedDeltaTime * transform.forward
            );
        }
        #endregion
    }

    #region Teams
    public int team = 1;

    public bool isAlly(PlayerMain otherPlayer) {
        return otherPlayer.team == this.team;
    }
    #endregion

    #region Motion
    [SerializeField]
    private float defSpeed = 0.3f;
    private bool advancing;
    private Rigidbody playerRigidBody;

    public void Advance() {
        if(!this.isAtacking()) advancing = true;
    }

    public void Stop() {
        advancing = false;
    }

    public void LookAt(Vector3 position, bool force = false) {
        if (force || !this.isAtacking())
        {
            LookTowards(position - transform.position, force);
        }
    }

    public void LookTowards(Vector3 direction, bool force = false)
    {
        if (force || !this.isAtacking())
        {
            direction.y = 0;
            playerRigidBody.MoveRotation(
                Quaternion.LookRotation(direction, transform.up)
            );
        }
    }
    #endregion

    #region Atack
    private int dashingLayer;
    private int playerLayer;
    
    public Transform hitAreaSpawnZone;

    public PlayerWeaponDef meleeWeaponDef;
    public PlayerWeaponDef chargedWeaponDef;
    public PlayerWeaponDef dashWeaponDef;

    private float meleeAttackCooldown;
    private float meleeAttackRestitution;

    private float dashCooldown;
    private float dashRestitution;

    private float chargedAttackCooldown;
    private float chargedAttackRestitution;

    public float chargingStartedAt = 0;

    public void Attack(Vector3 position) {
        if (!isAtacking() && meleeAttackCooldown <= 0) {
            Debug.DrawLine(transform.position, position, Color.red, 1f);
            this.Stop();
            meleeAttackCooldown = meleeWeaponDef.attackCooldown;
            meleeAttackRestitution = meleeWeaponDef.attackRestitution;
            LookAt(position, true);
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
            this.Stop();
            LookTowards(direction, true);
            StartCoroutine(ActivateDashMode());
        }
    }
    
    private IEnumerator ActivateDashMode() {
        yield return new WaitForFixedUpdate();
        dashCooldown = dashWeaponDef.attackCooldown;
        dashRestitution = dashWeaponDef.attackRestitution + Time.fixedDeltaTime/2;
        gameObject.layer = dashingLayer;
    }
    
    private IEnumerator FinishDashing() {
        yield return new WaitForFixedUpdate();
        gameObject.layer = playerLayer;
    }

    public void ChargeHeavyAttack()
    {
        if(!isAtacking() && chargedAttackCooldown <= 0 && !isChargingHeavyAttack())
        {
            chargingStartedAt = Time.time;
            this.Stop();
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
        hitArea.spawner = this;
        hitArea.playerWeaponDef = chargedWeaponDef;
    }

    public bool isChargingHeavyAttack()
    {  return chargingStartedAt != 0; }
    #endregion

    #region Health
    private LifeBar lifeBar;

    [SerializeField]
    private float defHitPoints = 100;

    public float hitPoints { get; private set; }

    public void DamageWith(PlayerWeaponDef weapon)
    {
        this.AddHitPoints(-weapon.attackDmg);
    }

    public void AddHitPoints(float hitPointsDiff)
    {
        this.hitPoints += hitPointsDiff;
        if(lifeBar)
        {
            lifeBar.setValue(LifeFraction());
        }

        if (hitPoints > defHitPoints)
        {
            hitPoints = defHitPoints;
        } else if(hitPoints <= 0)
        {
            Die();
        }
    }

    public float LifeFraction()
    {
        return hitPoints / defHitPoints;
    }

    public void Die()
    {
        gameObject.DestroyAPS();
    }
    #endregion

    #region Physics
    public void Push(Vector3 pushVector)
    {
        playerRigidBody.velocity += pushVector;
    }
    #endregion
}
