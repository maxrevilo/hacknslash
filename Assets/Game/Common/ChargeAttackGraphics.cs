using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMain))]
public class ChargeAttackGraphics : MonoBehaviour {

    private PlayerMain playerMain;
    public GameObject graphics;
    public GameObject releaseGraphics;

    private bool active = false;
    
	void Awake () {
        playerMain = GetComponent<PlayerMain>();
        if (graphics == null) throw new Exception("graphics not set");
        if (releaseGraphics == null) throw new Exception("releaseGraphics not set");
    }
	
	void Update () {
	    if(playerMain.isChargingHeavyAttack())
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
