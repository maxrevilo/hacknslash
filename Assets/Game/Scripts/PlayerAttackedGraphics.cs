using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
[RequireComponent(typeof(Animator))]
public class PlayerAttackedGraphics : MonoBehaviour {

    private PlayerConstitution playerConstitution;
    private Animator animator;

    void Awake()
    {
        playerConstitution = GetComponent<PlayerConstitution>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        playerConstitution.OnAttackedEvent += Attacked;
    }

    void Attacked(PlayerMain playerMain, PlayerWeaponDef weapon)
    {
        animator.SetTrigger("stunned");
    }
}
