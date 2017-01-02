using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerConstitution))]
[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Responsabilities:
/// - Definition, State and calculations of stability.
/// - Event triggering and physics effects of loss of stability (for animation effects see PlayerAttackedGraphics).
/// </summary>
public class PlayerStability: Resetable {

    public delegate void StunnedEvent();
    /// <summary>
    /// When the player is hitted but has enough stability to function normally.
    /// </summary>
    public event StunnedEvent OnStunnedEvent;

    public delegate void StunLockedEvent();
    /// <summary>
    /// When the player is stun-locked due to loss of stability
    /// </summary>
    public event StunLockedEvent OnStunLockedEvent;

    public delegate void KnockedBackEvent();
    /// <summary>
    /// When the player is Kocked back due to loss of stability
    /// </summary>
    public event KnockedBackEvent OnKnockedBackEvent;

    public delegate void ThrownEvent();
    /// <summary>
    /// When the player is thrown due to loss of stability
    /// </summary>
    public event ThrownEvent OnThrownEvent;

    public delegate void BackUpEvent();
    /// <summary>
    /// When the player is ready to get back on its feets after being thrown or knocked back.
    /// </summary>
    public event BackUpEvent OnBackUpEvent;

    /// <summary>
    /// States of the stability of the player
    /// </summary>
    public enum PlayerStabilityState { Stable, StunLocked, KnockedBack, Thrown };

    /// <summary>
    /// Current stability state of the player.
    /// </summary>
    public PlayerStabilityState state { get; private set; }

    /// <summary>
    /// Stability Hit Points.
    /// </summary>
    public float defStability = 60;

    /// <summary>
    /// Time after the last hit to recover full stability, if the player is hit this timer will be reset.
    /// </summary>
    public float defTimeToRecover = 2f;

    /// <summary>
    /// How long the player will last stun locked
    /// </summary>
    public float defStunLockDuration = 1f;

    public float defTimeToBackUp = 4f;

    /// <summary>
    /// Min percentage of stability that prevents the player from being stun-locked.
    /// </summary>
    public const float STUNLOCK_STABILITY = 0.5f;
    /// <summary>
    /// Min percentage of stability that prevents the player from being Knocked back.
    /// </summary>
    public const float KNOCKBACK_STABILITY = 0.35f;
    /// <summary>
    /// Min percentage of stability that prevents the player from being Knocked back.
    /// </summary>
    public const float THROWN_STABILITY = 0f;

    private float stability;
    private CountDown recoveryCounter;
    private CountDown stunLockCounter;
    private CountDown backUpCounter;
    private PlayerMain playerMain;
    private PlayerConstitution playerConstitution;
    private Rigidbody _rigidBody;

    protected override void Awake()
    {
        base.Awake();
        playerMain = GetComponent<PlayerMain>();
        playerConstitution = GetComponent<PlayerConstitution>();
        _rigidBody = GetComponent<Rigidbody>();

        playerConstitution.OnAttackedEvent += Hit;
    }

    protected override void OnDestroy() {
        playerConstitution.OnAttackedEvent -= Hit;
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
    }

    protected override void _Reset()
    {
        stability = defStability;
        state = PlayerStabilityState.Stable;

        recoveryCounter.Stop();
        stunLockCounter.Stop();
        backUpCounter.Stop();
    }

    // Update is called once per frame
    protected override void FixedUpdate() {
        base.FixedUpdate();
        switch (state)
        {
            case PlayerStabilityState.Stable:
                break;
            case PlayerStabilityState.StunLocked:
                if(stunLockCounter.HasFinished()) {
                    state = PlayerStabilityState.Stable;
                }
                break;
            case PlayerStabilityState.KnockedBack:
            case PlayerStabilityState.Thrown:
                if(backUpCounter.HasFinished()) {
                    state = PlayerStabilityState.Stable;
                    stability = defStability;
                    if (OnBackUpEvent != null) OnBackUpEvent();
                }
                break;
        }
        
        if (recoveryCounter.HasFinished())
        {
            stability = defStability;
            /*
            switch (state)
            {
                case PlayerStabilityState.Stable:
                    break;
                case PlayerStabilityState.StunLocked:
                    // This should not happen since defStunLockDuration < defTimeToRecover
                    throw new Exception("Invalid state");
                case PlayerStabilityState.KnockedBack:
                case PlayerStabilityState.Thrown:
                    state = PlayerStabilityState.Stable;
                    break;
            }
            */
        }
	}

    public bool IsStable() {
        return state == PlayerStabilityState.Stable;
    }

    public void Hit(HitArea hitArea)
    {
        PlayerWeaponDef weapon = hitArea.playerWeaponDef;

        stability = Mathf.Max(stability - weapon.attackDmg, 0f);

        if (stability <= defStability * THROWN_STABILITY) ThrowPlayer(hitArea);
        else if (stability <= defStability * KNOCKBACK_STABILITY) KnockBackPlayer(hitArea);
        else if (stability <= defStability * STUNLOCK_STABILITY) StunLockPlayer(hitArea);
        else StunPlayer(hitArea);
    }

    private void ThrowPlayer(HitArea hitArea) {
        PlayerWeaponDef weapon = hitArea.playerWeaponDef;

        switch(state)
        {
            case PlayerStabilityState.Stable:
            case PlayerStabilityState.StunLocked:
            case PlayerStabilityState.KnockedBack:
                state = PlayerStabilityState.Thrown;
                backUpCounter.Restart(defTimeToBackUp);
                recoveryCounter.Restart(defTimeToRecover);
                if (OnThrownEvent != null) OnThrownEvent();
                break;
        }

        _rigidBody.AddForce(hitArea.CalculateHitImpulse(playerMain), ForceMode.Impulse);
    }

    private void KnockBackPlayer(HitArea hitArea) {
        switch (state)
        {
            case PlayerStabilityState.Stable:
            case PlayerStabilityState.StunLocked:
                state = PlayerStabilityState.KnockedBack;
                recoveryCounter.Restart(defTimeToRecover);
                backUpCounter.Restart(defTimeToBackUp);
                if (OnKnockedBackEvent != null) OnKnockedBackEvent();
                break;
            case PlayerStabilityState.KnockedBack:
                recoveryCounter.Restart(defTimeToRecover);
                break;
        }
    }

    private void StunLockPlayer(HitArea hitArea) {
        switch (state)
        {
            case PlayerStabilityState.Stable:
            case PlayerStabilityState.StunLocked:
                state = PlayerStabilityState.StunLocked;
                recoveryCounter.Restart(defTimeToRecover);
                stunLockCounter.Restart(defStunLockDuration);

                if (OnStunLockedEvent != null) OnStunLockedEvent();

                break;
        }
    }

    private void StunPlayer(HitArea hitArea) {
        switch (state)
        {
            case PlayerStabilityState.Stable:
                state = PlayerStabilityState.Stable;
                recoveryCounter.Restart(defTimeToRecover);
                if (OnStunnedEvent != null) OnStunnedEvent();
                break;
        }
    }
}
