using System;
using UnityEngine;
using System.Collections;

public class HitArea : MonoBehaviour
{
    // public delegate void HitEvent();
    // public static event HitEvent OnHitEvent;

    public PlayerMain spawner { get; set; }
    public PlayerWeaponDef playerWeaponDef { get; set; }

    private float timeOfLife;

    void Reset()
    {
        timeOfLife = 0;
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
            //Debug.LogFormat("HitArea: Hit on {0}", other.gameObject.name
            playerHit.DamageWith(playerWeaponDef);

            switch (playerWeaponDef.projectileType)
            {
                case PlayerWeaponDef.ProjectileType.SingleHit:
                    this.Destroy();
                    break;
                case PlayerWeaponDef.ProjectileType.AreaDamage:
                    throw new NotImplementedException();
                case PlayerWeaponDef.ProjectileType.AreaDamageOverTime:
                    break;
            }
        }
    }

    void Destroy()
    {
        gameObject.DestroyAPS();
        Reset();
    }
}
