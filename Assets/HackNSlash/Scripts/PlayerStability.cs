using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerConstitution))]
[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Responsabilities:
/// - Definition, State and calculations of stability.
/// - Event triggering and physics effects of loss of stability (for animation effects see PlayerAttackedGraphics).
/// </summary>
public class PlayerStability: MonoBehaviour {

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
    public PlayerStabilityState state;

    /// <summary>
    /// Stability Hit Points.
    /// </summary>
    public float defStability = 60;
    /// <summary>
    /// Time after the last hit to recover full stability, if the player is hit this timer will be reset.
    /// </summary>
    public float defTimeToRecover = 2f;

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

    public float stability;
    public float recoverTimer;
    private PlayerMain playerMain;
    private PlayerConstitution playerConstitution;
    private Rigidbody _rigidBody;

    void Awake()
    {
        playerMain = GetComponent<PlayerMain>();
        playerConstitution = GetComponent<PlayerConstitution>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    void Start () {
        stability = defStability;
        recoverTimer = 0f;
        state = PlayerStabilityState.Stable;

        playerConstitution.OnAttackedEvent += Hit;
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        recoverTimer -= Time.fixedDeltaTime;
        if (recoverTimer <= 0f)
        {
            recoverTimer = 0f;
            stability = defStability;

            switch (state)
            {
                case PlayerStabilityState.Stable:
                    break;
                case PlayerStabilityState.StunLocked:
                    state = PlayerStabilityState.Stable;
                    break;
                case PlayerStabilityState.KnockedBack:
                case PlayerStabilityState.Thrown:
                    state = PlayerStabilityState.Stable;
                    if (OnBackUpEvent != null) OnBackUpEvent();
                    break;
            }
        }
	}

    public void Hit(HitArea hitArea)
    {
        PlayerWeaponDef weapon = hitArea.playerWeaponDef;

        stability = Mathf.Max(stability - weapon.attackDmg, 0f);

        //TODO: This is really hard to read
        if (stability <= defStability * THROWN_STABILITY)
        {
            switch(state)
            {
                case PlayerStabilityState.Stable:
                case PlayerStabilityState.StunLocked:
                case PlayerStabilityState.KnockedBack:
                    state = PlayerStabilityState.Thrown;
                    recoverTimer = defTimeToRecover;
                    if (OnThrownEvent != null) OnThrownEvent();
                    break;
            }

            Vector3 directionalPushVector = hitArea.transform.forward * weapon.directionalPushStrenght;
            Vector3 vectorToPlayer = Vector3.Normalize(playerMain.transform.position - hitArea.transform.position);
            Vector3 radialPushVector = vectorToPlayer * weapon.radialPushStrenght;
            Vector3 elevatingPushVector = Vector3.up * weapon.elevatingPushStrenght;
            Vector3 finalImpulse = directionalPushVector + radialPushVector + elevatingPushVector; 
            _rigidBody.AddForce(finalImpulse, ForceMode.Impulse);
            playerMain.transform.rotation.SetLookRotation(finalImpulse, transform.up);
        }
        else if (stability <= defStability * KNOCKBACK_STABILITY)
        {
            switch (state)
            {
                case PlayerStabilityState.Stable:
                case PlayerStabilityState.StunLocked:
                    state = PlayerStabilityState.KnockedBack;
                    recoverTimer = defTimeToRecover;
                    if (OnKnockedBackEvent != null) OnKnockedBackEvent();
                    break;
                case PlayerStabilityState.KnockedBack:
                    recoverTimer = defTimeToRecover;
                    break;
            }
        }
        else if (stability <= defStability * STUNLOCK_STABILITY)
        {
            switch (state)
            {
                case PlayerStabilityState.Stable:
                case PlayerStabilityState.StunLocked:
                    state = PlayerStabilityState.StunLocked;
                    recoverTimer = defTimeToRecover;
                    if (OnStunLockedEvent != null) OnStunLockedEvent();
                    break;
            }
        } else
        {
            switch (state)
            {
                case PlayerStabilityState.Stable:
                    state = PlayerStabilityState.Stable;
                    recoverTimer = defTimeToRecover;
                    if (OnStunnedEvent != null) OnStunnedEvent();
                    break;
            }
        }
    }
}
