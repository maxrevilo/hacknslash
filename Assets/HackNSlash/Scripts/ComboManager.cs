using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BattleGameScene))]
public class ComboManager : Resetable {
    public delegate void ComboIncreasedEvent(float comboDamage);
    public event ComboIncreasedEvent OnComboIncreasedEvent;

    public delegate void ComboFinishedEvent(float totalComboDamage);
    public event ComboFinishedEvent OnComboFinishedEvent;

    public delegate void HitRegistered(HitArea hit);
    public event HitRegistered OnHitRegistered;

    BattleGameScene battleScene;

    public float dmgInCombo { get; private set; }
    [SerializeField]
    private float timeToLooseCombo = 3f;
    public CountDown closeComboTimer;

    public float bestCombo { get; private set; }

    protected override void Start () {
        base.Start();
        battleScene = GetComponent<BattleGameScene>();
    }

    protected override void _Reset()
    {
        dmgInCombo = 0;
        closeComboTimer.Stop();
        bestCombo = 0;
    }

    protected override void FixedUpdate ()
    {
        base.FixedUpdate();

        if(dmgInCombo > 0 && closeComboTimer.HasFinished())
        {
            if (OnComboFinishedEvent != null) OnComboFinishedEvent(dmgInCombo);
            if (bestCombo < dmgInCombo) bestCombo = dmgInCombo;
            dmgInCombo = 0;
        }
    }

    public void RegisterHit(HitArea hit)
    {
        if (OnHitRegistered != null) OnHitRegistered(hit);

        if (hit.spawner == battleScene.mainPlayer)
        {
            dmgInCombo += hit.playerWeaponDef.attackDmg;
            closeComboTimer.Restart(timeToLooseCombo);
            if (OnComboIncreasedEvent != null) OnComboIncreasedEvent(dmgInCombo);
        }
    }

    public float timeFractionLeft()
    {
        return closeComboTimer.TimeToFinish() / timeToLooseCombo;
    }

}
