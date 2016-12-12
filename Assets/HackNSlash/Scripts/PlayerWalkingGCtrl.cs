using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMotion))]
[RequireComponent(typeof(Animator))]
public class PlayerWalkingGCtrl : MonoBehaviour {

    private PlayerMotion playerMotion;
    private Animator animator;
    
	void Awake () {
        playerMotion = GetComponent<PlayerMotion>();
        animator = GetComponent<Animator>();
    }

	void Start () {
        playerMotion.OnMovingEvent += MovingEvent;
    }
	
    void MovingEvent(PlayerMain playerMain, bool moving) {
        animator.SetBool("moving", moving);
    }
}
