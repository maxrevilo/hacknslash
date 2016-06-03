using System;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
public class ChargeAttackGraphics : MonoBehaviour {

    private PlayerAttack playerAttack;
    public GameObject graphics;
    public GameObject releaseGraphics;

    private bool active = false;
    
	void Awake () {
        playerAttack = GetComponent<PlayerAttack>();
        if (graphics == null) throw new Exception("graphics not set");
        if (releaseGraphics == null) throw new Exception("releaseGraphics not set");
    }
	
	void Update () {
	    if(playerAttack.isChargingHeavyAttack())
        {
            if(!active)
            {
                active = true;
                graphics.SetActive(true);
                releaseGraphics.SetActive(false);
            }
        } else
        {
            if(active)
            {
                releaseGraphics.SetActive(true);
            }
            active = false;
            graphics.SetActive(false);
        }
	}
}
