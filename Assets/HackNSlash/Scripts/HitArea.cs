using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class HitArea : Resetable
{
    // public delegate void HitEvent();
    // public static event HitEvent OnHitEvent;

    public PlayerMain spawner { get; set; }
    public PlayerWeaponDef playerWeaponDef { get; set; }

    public Vector3 originalScale;
    private HashSet<PlayerMain> playersReached;

    private ComboManager comboManager;

    private float timeOfLife;

    protected override void Awake()
    {
        base.Awake();
        playersReached = new HashSet<PlayerMain>();
        originalScale = transform.localScale;
    }

    protected override void Start ()
    {
        base.Start();
        Assert.IsNotNull(playerWeaponDef, "playerWeaponDef not set");
        comboManager = GetComponentInParent<ComboManager>();
    }

    protected override void _Reset()
    {
        timeOfLife = 0;
        playersReached.Clear();
        transform.localScale = originalScale;
    }

    protected override void Update ()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        timeOfLife += Time.fixedDeltaTime;

        if(timeOfLife >= playerWeaponDef.hitAreaLifeSpan)
        {
            this.Destroy();
        }
    }

    public Vector3 CalculateHitImpulse(PlayerMain player)
    {
        Vector3 directionalPushVector = this.transform.forward * playerWeaponDef.directionalPushStrenght;
        Vector3 vectorToPlayer = Vector3.Normalize(player.transform.position - this.transform.position);
        Vector3 radialPushVector = vectorToPlayer * playerWeaponDef.radialPushStrenght;
        Vector3 elevatingPushVector = Vector3.up * playerWeaponDef.elevatingPushStrenght;
        Vector3 finalImpulse = directionalPushVector + radialPushVector + elevatingPushVector;

        return finalImpulse;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == spawner.gameObject) return;

        PlayerMain playerHit = other.GetComponent<PlayerMain>();

        if(playerHit != null && !spawner.isAlly(playerHit))
        {
            //Debug.LogFormat("HitArea: Hit on {0}", other.gameObject.name);
            bool doHit = false;

            switch (playerWeaponDef.projectileType)
            {
                case PlayerWeaponDef.ProjectileType.SingleHit:
                    doHit = true;
                    this.Destroy();
                    break;
                case PlayerWeaponDef.ProjectileType.AreaDamage:
                    if(!playersReached.Contains(playerHit))
                    {
                        playersReached.Add(playerHit);
                        doHit = true;
                    }
                    break;
                case PlayerWeaponDef.ProjectileType.AreaDamageOverTime:
                    doHit = true;
                    break;
            }

            if(doHit)
            {
                PlayerConstitution playerHitConstitution = other.GetComponent<PlayerConstitution>();
                playerHitConstitution.DamageBy(this);
                if(comboManager != null) comboManager.RegisterHit(this);
            }
        }
    }

    void Destroy()
    {
        gameObject.DestroyAPS();
        ResetComponent();
    }
}
