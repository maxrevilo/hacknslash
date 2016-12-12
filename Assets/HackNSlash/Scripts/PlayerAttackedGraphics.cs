using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStability))]
/// <summary>
/// Responsabilities:
/// - Listening of death event from PlayerHealth and play of animation.
/// - Listening of loss of stability events from PlayerStability
/// </summary>
public class PlayerAttackedGraphics : MonoBehaviour {
    private PlayerConstitution playerConstitution;
    private PlayerStability playerStability;
    private Animator animator;
    private int stunnedHash;
    private int stunLockedHash;
    private int knockedBackHash;
    private int thrownHash;
    private int deadHash;
    private int backUpHash;

    void Awake()
    {
        playerConstitution = GetComponent<PlayerConstitution>();
        playerStability = GetComponent<PlayerStability>();
        animator = GetComponent<Animator>();

        stunnedHash = Animator.StringToHash("stunned");
        stunLockedHash = Animator.StringToHash("stun_locked");
        knockedBackHash = Animator.StringToHash("knocked_back");
        thrownHash = Animator.StringToHash("thrown");
        deadHash = Animator.StringToHash("dead");
        backUpHash = Animator.StringToHash("back_up");
    }

    void Start()
    {
        playerConstitution.OnDieEvent += Dying;
        playerStability.OnStunnedEvent += Stunned;
        playerStability.OnStunLockedEvent += StunLocked;
        playerStability.OnKnockedBackEvent += KnockedBack;
        playerStability.OnThrownEvent += Thrown;
        playerStability.OnBackUpEvent += BackUp;
    }

    void Stunned()
    {
        animator.SetTrigger(stunnedHash);
    }

    void StunLocked()
    {
        animator.SetTrigger(stunLockedHash);
    }

    void KnockedBack()
    {
        animator.SetTrigger(knockedBackHash);
    }

    void Thrown()
    {
        animator.applyRootMotion = false;
        animator.SetTrigger(thrownHash);
    }

    void BackUp()
    {
        animator.applyRootMotion = true;
        animator.SetTrigger(backUpHash);
    }

    void Dying(PlayerMain playerMain, float lastHit)
    {
        animator.SetTrigger(deadHash);
    }
}
