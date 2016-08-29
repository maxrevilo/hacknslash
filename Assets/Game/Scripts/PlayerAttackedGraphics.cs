using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
[RequireComponent(typeof(Animator))]
public class PlayerAttackedGraphics : MonoBehaviour {

    private PlayerConstitution playerConstitution;
    private Animator animator;

    private int stunnedHash;
    private int deadHash;

    void Awake()
    {
        playerConstitution = GetComponent<PlayerConstitution>();
        animator = GetComponent<Animator>();

        stunnedHash = Animator.StringToHash("stunned");
        deadHash = Animator.StringToHash("dead");
    }

    void Start()
    {
        playerConstitution.OnAttackedEvent += Attacked;
        playerConstitution.OnDieEvent += Dying;
    }

    void Attacked(PlayerMain playerMain, PlayerWeaponDef weapon)
    {
        animator.SetTrigger(stunnedHash);
    }

    void Dying(PlayerMain playerMain, float lastHit)
    {
        animator.SetTrigger(deadHash);
    }
}
