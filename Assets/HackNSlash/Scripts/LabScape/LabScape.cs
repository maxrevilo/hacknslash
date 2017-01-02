using System;
using UnityEngine;

public class LabScape : BattleGameScene
{
    public PlayerMain lastBoss;

    protected override void Awake()
    {
        base.Awake();
        if(lastBoss == null) throw new Exception("lastBoss not set");
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }
}