using System;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
public class ChargeAttackGraphics : MonoBehaviour {

    private PlayerAttack playerAttack;
    public GameObject graphics;
    public GameObject releaseGraphics;
    
    private ParticleSystem[] chargingPS;

    // private bool active = false;
    
	void Awake () {
        playerAttack = GetComponent<PlayerAttack>();
        if (graphics == null) throw new Exception("graphics not set");
        if (releaseGraphics == null) throw new Exception("releaseGraphics not set");
        
        chargingPS = graphics.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in chargingPS) ps.Stop();
    }
	void Start () {
        playerAttack.OnChargingHeavyAttackEvent += ChargeHeavyAttack;
        playerAttack.OnReleaseHeavyAttackEvent += ReleaseHeavyAttackEvent;
    }
	
    void ChargeHeavyAttack(PlayerMain playerMain) {
        foreach (ParticleSystem ps in chargingPS) ps.Play();
        releaseGraphics.SetActive(false);
    }
    
    void ReleaseHeavyAttackEvent(PlayerMain playerMain, bool fullyCharged) {
        foreach (ParticleSystem ps in chargingPS) ps.Stop();
        if(fullyCharged) {
            releaseGraphics.SetActive(true);
        }
    }
    
	void Update () {
	    // if(playerAttack.isChargingHeavyAttack())
        // {
        //     if(!active)
        //     {
        //         active = true;
        //         graphics.SetActive(true);
        //         releaseGraphics.SetActive(false);
        //     }
        // } else
        // {
        //     if(active)
        //     {
        //         releaseGraphics.SetActive(true);
        //     }
        //     active = false;
        //     graphics.SetActive(false);
        // }
	}
}
