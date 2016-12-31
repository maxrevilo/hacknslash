using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(Animator))]
public class PlayerAttackingGraphics : Resetable {

    private PlayerAttack playerAttack;
    private Animator animator;

    private int attackingHash;
    private int dashingHash;

    public bool attackingTrigger = false;

    protected override void Awake () {
        base.Awake();
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();

        attackingHash = Animator.StringToHash("attacking");
        dashingHash = Animator.StringToHash("dashing");
    }

    protected override void Start () {
        base.Start();
        playerAttack.OnAttackingEvent += Attacking;
        playerAttack.OnDashingEvent += Dashing;
    }

    protected override void _Reset()
    {
        animator.Rebind();
    }

    void Attacking(PlayerMain playerMain, PlayerWeaponDef weapon) {
        animator.SetTrigger(attackingHash);
        if(gameObject.name.Equals("PlayerChan")) {
            Debug.Log("Trigger Attack");
        }
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
