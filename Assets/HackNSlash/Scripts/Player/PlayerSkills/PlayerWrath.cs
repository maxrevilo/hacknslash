using System;
using UnityEngine;


[RequireComponent(typeof(PlayerMain))]
public class PlayerWrath : PlayerSkillResource
{
    [SerializeField]
    public float gainPerHit = 7;
    [SerializeField]
    public float gainPerSecond = -2;

    private ComboManager comboManager;

    protected override void Awake()
    {
        comboManager = GetComponentInParent<ComboManager>();
        base.Awake();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        AddToAmount(gainPerSecond * Time.fixedDeltaTime);
    }

    protected override void _Reset()
    {
        comboManager.OnHitRegistered -= GlobalHitRegistered;
        comboManager.OnHitRegistered += GlobalHitRegistered;
        amount = 0;
    }

    private void GlobalHitRegistered(HitArea hit)
    {
        if (hit.spawner == playerMain)
        {
            AddToAmount(gainPerHit);
        }
    }
}
