using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerWeaponDef {
    public enum ProjectileType {
        SingleHit,
        AreaDamage,
        AreaDamageOverTime
    };

    public float attackDmg = 0;
    public ProjectileType projectileType = ProjectileType.SingleHit;
    public float hitAreaLifeSpan = 0.1f;
    public float attackCooldown = 0.2f;
    public float attackRestitution = 0.2f;

    public float directionalPushStrenght = 1f;
    public float radialPushStrenght = 0f;
    public float elevatingPushStrenght = 0f;

    //Charged
    public float chargeTime = 1.5f;
}
