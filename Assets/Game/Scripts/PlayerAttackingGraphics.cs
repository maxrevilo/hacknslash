using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(Animator))]
public class PlayerAttackingGraphics : MonoBehaviour {

    private PlayerAttack playerAttack;
    private Animator animator;

    private int attackingHash;
    private int dashingHash;

    void Awake () {
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();

        attackingHash = Animator.StringToHash("attacking");
        dashingHash = Animator.StringToHash("dashing");
    }

	void Start () {
        playerAttack.OnAttackingEvent += Attacking;
        playerAttack.OnDashingEvent += Dashing;
    }
	
    void Attacking(PlayerMain playerMain, PlayerWeaponDef weapon) {
        animator.SetTrigger(attackingHash);
    }

    void Dashing(PlayerMain playerMain, PlayerWeaponDef weapon)
    {
        animator.SetTrigger(dashingHash);
    }

    public void StartHitMoment()
    {
        playerAttack.ActivateAtackArea();
    }
}
