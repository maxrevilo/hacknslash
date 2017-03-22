using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerWeaponDef {
    public enum ProjectileType {
        // Only hits one reached enemy and then the hit area is disabled.
        SingleHit,
        // Hits all reached enemies once
        AreaDamage,
        // Hits all reached enemies while they are in reach of the hit area.
        AreaDamageOverTime
    };

    // The Damage inflicted by the attack
    public float attackDmg = 0;
    // The type of the projectile generated
    public ProjectileType projectileType = ProjectileType.SingleHit;
    // The amount time the hit area is activated on the scenario.
    public float hitAreaLifeSpan = 0.1f;
    // The amout of time that must be waited before attacking again.
    public float attackCooldown = 0.2f;
    // The amout of time in which the player is attacking and can't do anything else.
    public float attackRestitution = 0.2f;
    // The amout of time before the attack area apears (ak. the player actually attacks)
    public float attackPreparation = 0.5f;

    // The force applied to enemies reached in the forward direction of the attack.
    public float directionalPushStrenght = 1f;
    // The force applied to enemies reached in the radial direction from the center of the attack and the enemy.
    public float radialPushStrenght = 0f;
    // The force applied to enemies reached in the up direction of the attack.
    public float elevatingPushStrenght = 0f;

    // The amout of time that the player must charge this attack.
    public float chargeTime = 1.5f;

    // If it's a dash, the distance reached by the dash.
    public float dashDistance = 15f;

    // If the player has an skill resource this is the cost to do this attack
    public float skillCost = 0f;
}
