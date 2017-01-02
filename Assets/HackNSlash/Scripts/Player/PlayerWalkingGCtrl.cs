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
        playerMotion.OnMovingEvent += MovingEvent;
    }

    protected override void OnDestroy() {
        playerMotion.OnMovingEvent -= MovingEvent;
    }

    protected override void Start () {
        base.Start();
    }
	
    void MovingEvent(PlayerMain playerMain, bool moving) {
        animator.SetBool("moving", moving);
    }

    protected override void _Reset()
    {
    }
}
