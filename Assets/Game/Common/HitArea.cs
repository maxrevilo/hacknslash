using System;
using UnityEngine;
using System.Collections.Generic;

public class HitArea : MonoBehaviour
{
    // public delegate void HitEvent();
    // public static event HitEvent OnHitEvent;

    public PlayerMain spawner { get; set; }
    public PlayerWeaponDef playerWeaponDef { get; set; }

    private HashSet<PlayerMain> playersReached;

    private float timeOfLife;

    void Reset()
    {
        timeOfLife = 0;
        playersReached.Clear();
    }

    void Awake()
    {
        playersReached = new HashSet<PlayerMain>();
    }

    void Start ()
    {
        if (playerWeaponDef == null) throw new Exception("playerWeaponDef not set");
    }

	void Update ()
    {
    }

    void FixedUpdate()
    {
        timeOfLife += Time.fixedDeltaTime;

        if(timeOfLife >= playerWeaponDef.hitAreaLifeSpan)
        {
            this.Destroy();
        }
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
                playerHitConstitution.DamageWith(playerWeaponDef);

                Vector3 directionalPushVector = transform.forward * playerWeaponDef.directionalPushStrenght;
                Vector3 vectorToPlayer = Vector3.Normalize(playerHit.transform.position - transform.position);
                Vector3 radialPushVector = vectorToPlayer * playerWeaponDef.radialPushStrenght;
                Vector3 elevatingPushVector = Vector3.up * playerWeaponDef.elevatingPushStrenght;
                playerHit.Push(directionalPushVector + radialPushVector + elevatingPushVector);
            }
        }
    }

    void Destroy()
    {
        gameObject.DestroyAPS();
        Reset();
    }
}
