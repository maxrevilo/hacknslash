using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerStability))]
[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Responsabilities:
/// - Listening of death event from PlayerHealth and play of animation.
/// - Listening of loss of stability events from PlayerStability
/// </summary>
public class PlayerAttackedGraphics : MonoBehaviour {
    private PlayerConstitution playerConstitution;
    private PlayerStability playerStability;
    private Rigidbody _rigidBody;

    private Animator animator;
    private int stunnedHash;
    private int stunLockedHash;
    private int knockedBackHash;
    private int thrownHash;
    private int deadHash;
    private int backUpHash;
    private int stunLockedSpeedHash;
    private int backUpSpeedHash;

    private CountDown timerToplayBackUp;
    private bool isOutOfSelfControl;

    public float backUpClipLength = 0.86f;

    void Awake()
    {
        playerConstitution = GetComponent<PlayerConstitution>();
        playerStability = GetComponent<PlayerStability>();
        animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();

        stunnedHash = Animator.StringToHash("stunned");
        stunLockedHash = Animator.StringToHash("stun_locked");
        knockedBackHash = Animator.StringToHash("knocked_back");
        thrownHash = Animator.StringToHash("thrown");
        deadHash = Animator.StringToHash("dead");
        backUpHash = Animator.StringToHash("back_up");
        stunLockedSpeedHash = Animator.StringToHash("stun_locked_speed");
        backUpSpeedHash = Animator.StringToHash("back_up_speed");

        playerConstitution.OnDieEvent += Dying;
        playerStability.OnStunnedEvent += Stunned;
        playerStability.OnStunLockedEvent += StunLocked;
        playerStability.OnKnockedBackEvent += KnockedBack;
        playerStability.OnThrownEvent += Thrown;
    }

    void Start()
    {
        timerToplayBackUp.Stop();
        isOutOfSelfControl = false;
    }

    void Update()
    {
        if(isOutOfSelfControl) {
            Vector3 direction = -_rigidBody.velocity + transform.forward * 0.1f;
            direction.y *= 0f;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, transform.up),
                Time.deltaTime * 30f
            );
            if(isOutOfSelfControl && timerToplayBackUp.HasFinished()) {
                BackUp();
            }
        }
        
    }

    void Stunned()
    {
        animator.SetTrigger(stunnedHash);
    }

    void StunLocked()
    {
        float speed = 1f / playerStability.defStunLockDuration;
        animator.SetFloat(stunLockedSpeedHash, speed);
        animator.SetTrigger(stunLockedHash);
    }

    void KnockedBack()
    {
        animator.SetTrigger(knockedBackHash);
        isOutOfSelfControl = true;
        timerToplayBackUp.Restart(playerStability.defTimeToBackUp - backUpClipLength);
    }

    void Thrown()
    {
        animator.applyRootMotion = false;
        animator.SetTrigger(thrownHash);
        isOutOfSelfControl = true;
        timerToplayBackUp.Restart(playerStability.defTimeToBackUp - backUpClipLength);
    }

    void BackUp()
    {
        isOutOfSelfControl = false;
        animator.applyRootMotion = true;
        animator.SetTrigger(backUpHash);
    }

    void Dying(PlayerMain playerMain, float lastHit)
    {
        animator.SetTrigger(deadHash);
    }
}
