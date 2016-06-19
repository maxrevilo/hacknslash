using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerConstitution))]
public class PlayerAttackedGraphics : MonoBehaviour {

    private PlayerConstitution playerConstitution;
    
    private ParticleSystem[] chargingPS;

    [SerializeField]
    private Animator _animation;

    // private bool active = false;
    
	void Awake () {
        playerConstitution = GetComponent<PlayerConstitution>();
        if (_animation == null) throw new Exception("animation not set");
    }
	void Start () {
        playerConstitution.OnAttackedEvent += Attacked;
    }
	
    void Attacked(PlayerMain playerMain, PlayerWeaponDef weapon) {
        _animation.SetTrigger("attacked");
    }

    IEnumerator Play() {
        _animation.enabled = false;
        yield return new WaitForEndOfFrame();
        _animation.enabled = true;
    }
    
	void Update () {}
}
