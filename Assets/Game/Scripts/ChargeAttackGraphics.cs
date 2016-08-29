﻿using System;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(Animator))]
public class ChargeAttackGraphics : MonoBehaviour {

    private PlayerAttack playerAttack;
    private Animator animator;
    public GameObject graphics;
    public GameObject releaseGraphics;
    
    private ParticleSystem[] chargingPS;

    // private bool active = false;
    private int chargingHash;
    private int releaseChargeHash;
    
	void Awake () {
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
        if (graphics == null) throw new Exception("graphics not set");
        if (releaseGraphics == null) throw new Exception("releaseGraphics not set");
        
        chargingPS = graphics.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in chargingPS) ps.Stop();

        chargingHash = Animator.StringToHash("charging");
        releaseChargeHash = Animator.StringToHash("release_charge");
    }
	void Start () {
        playerAttack.OnChargingHeavyAttackEvent += ChargeHeavyAttack;
        playerAttack.OnReleaseHeavyAttackEvent += ReleaseHeavyAttackEvent;
    }
	
    void ChargeHeavyAttack(PlayerMain playerMain) {
        foreach (ParticleSystem ps in chargingPS) ps.Play();
        releaseGraphics.SetActive(false);

        animator.SetBool(chargingHash, true);
    }
    
    void ReleaseHeavyAttackEvent(PlayerMain playerMain, bool fullyCharged) {
        foreach (ParticleSystem ps in chargingPS) ps.Stop();
        if(fullyCharged) {
            animator.SetTrigger(releaseChargeHash);
        }
        animator.SetBool(chargingHash, false);
    }

    public void StartChargedHitMoment()
    {
        StartCoroutine(playerAttack.ActivateChargedAttackArea());
        releaseGraphics.SetActive(true);
    }
    
	void Update () {}
}
