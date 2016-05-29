using UnityEngine;
using System.Collections;

public class PlayerWeaponDef : MonoBehaviour {
    public enum ProjectileType {
        SingleHit,
        AreaDamage,
        AreaDamageOverTime
    };

    public float attackDmg = 0;
    public ProjectileType projectileType = ProjectileType.SingleHit;
    public float hitAreaLifeSpan = 0.1f;
}
