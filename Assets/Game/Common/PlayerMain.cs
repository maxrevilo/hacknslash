using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour {
    void Awake()
    {
        #region Atack
        if (hitAreaSpawnZone == null) throw new Exception("hitAreaSpawnZone not set");
        playerWeaponDef = GetComponent<PlayerWeaponDef>();
        #endregion
        #region Health
        lifeBar = GetComponentInChildren<LifeBar>();
        #endregion
    }

    void Start() {
        #region Atack
        currentAttackCooldown = 0;
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
        currentAttackCooldown -= Time.fixedDeltaTime;
        #endregion
        #region Motion
        if (advancing) {
            playerRigidBody.MovePosition(
                transform.position + defSpeed * transform.forward
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
    public Transform hitAreaSpawnZone;

    [SerializeField]
    private float defAttackCooldown = 1;

    PlayerWeaponDef playerWeaponDef;

    private float currentAttackCooldown;

    public float chargingStartedAt = 0;

    public void Attack(Vector3 position) {
        if (!isAtacking()) {
            Debug.DrawLine(transform.position, position, Color.red, 0.05f);
            this.Stop();
            currentAttackCooldown = defAttackCooldown;
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
        hitArea.playerWeaponDef = playerWeaponDef;
    }

    public bool isAtacking()
    {
        return currentAttackCooldown > 0;
    }

    public void ChargeHeavyAttack()
    {
        if(!isChargingHeavyAttack())
        {
            chargingStartedAt = Time.time;
            this.Stop();
        }
    }

    public void ReleaseHeavyAttack()
    {
        chargingStartedAt = 0;
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
}
