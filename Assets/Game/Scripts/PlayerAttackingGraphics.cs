using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(Animator))]
public class PlayerAttackingGraphics : MonoBehaviour {

    private PlayerAttack playerAttack;
    private Animator animator;
    
	void Awake () {
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
    }

	void Start () {
        playerAttack.OnAttackingEvent += Attacking;
    }
	
    void Attacking(PlayerMain playerMain, PlayerWeaponDef weapon) {
        animator.SetTrigger("attacking");
    }
}
