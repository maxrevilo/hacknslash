using System;
using UnityEngine;


[RequireComponent(typeof(PlayerMain))]
public abstract class PlayerSkillResource : Resetable
{
    [SerializeField]
    public float maxAmount = 100; //Def
    [SerializeField]
    public float amount = 100;

    protected PlayerMain playerMain;

    protected override void Awake()
    {
        playerMain = GetComponent<PlayerMain>();
        base.Awake();
    }

    public void AddToAmount(float value)
    {
        amount = Mathf.Clamp(value + amount, 0, maxAmount);
    }

    public float GetFractionAmount() { return amount / maxAmount; }
}
