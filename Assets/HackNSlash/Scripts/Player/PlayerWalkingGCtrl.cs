using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMotion))]
[RequireComponent(typeof(Animator))]
public class PlayerWalkingGCtrl : Resetable
{

    private PlayerMotion playerMotion;
    private Animator animator;

    protected override void Awake() {
        base.Awake();
        playerMotion = GetComponent<PlayerMotion>();
        animator = GetComponent<Animator>();
    }

    protected override void Start () {
        base.Start();
        playerMotion.OnMovingEvent += MovingEvent;
    }
	
    void MovingEvent(PlayerMain playerMain, bool moving) {
        animator.SetBool("moving", moving);
    }

    protected override void _Reset()
    {
    }
}
