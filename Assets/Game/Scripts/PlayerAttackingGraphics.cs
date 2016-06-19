using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAttack))]
public class PlayerAttackingGraphics : MonoBehaviour {

    private PlayerAttack playerAttack;
    
    private ParticleSystem[] chargingPS;

    [SerializeField]
    private Animator _animation;

    // private bool active = false;
    
	void Awake () {
        playerAttack = GetComponent<PlayerAttack>();
        if (_animation == null) throw new Exception("animation not set");
    }

	void Start () {
        playerAttack.OnAttackingEvent += Attacking;
    }
	
    void Attacking(PlayerMain playerMain, PlayerWeaponDef weapon) {
        _animation.SetTrigger("attacking");
    }

    IEnumerator Play() {
        _animation.enabled = false;
        yield return new WaitForEndOfFrame();
        _animation.enabled = true;
    }
    
	void Update () {}
}
