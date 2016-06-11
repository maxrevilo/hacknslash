using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
public class PlayerAttackedGraphics : MonoBehaviour {

    private PlayerConstitution playerConstitution;
    
    private ParticleSystem[] chargingPS;

    [SerializeField]
    private Animator animation;

    // private bool active = false;
    
	void Awake () {
        playerConstitution = GetComponent<PlayerConstitution>();
        if (animation == null) throw new Exception("animation not set");
        // animation.Stop();
    }
	void Start () {
        playerConstitution.OnAttackedEvent += Attacked;
    }
	
    void Attacked(PlayerMain playerMain, PlayerWeaponDef weapon) {
        animation.SetTrigger("attacked");
        // StartCoroutine(Play());
    }

    IEnumerator Play() {
        animation.enabled = false;
        yield return new WaitForEndOfFrame();
        animation.enabled = true;
    }
    
	void Update () {}
}
