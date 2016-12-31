using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Resetable {
    [SerializeField]
    private CollisionPub trigger;
    [SerializeField]
    private LayerMask filterLayers;
    [SerializeField]
    private string filterTag = null;

    private Animator animator;
    int moveHash;

    protected override void _Reset()
    {
        animator.ResetTrigger(moveHash);
        animator.Rebind();
    }

    protected override void Awake () {
        base.Awake();
        if (trigger == null) throw new Exception("trigger not set");
        animator = GetComponent<Animator>();
        moveHash = Animator.StringToHash("move");
    }

    protected override void Start()
    {
        base.Start();
        trigger.OnTriggerEnterEvent += TiggerEnter;
        trigger.OnTriggerExitEvent += TiggerExit;
    }

    private void TiggerEnter(Collider other)
    {
        //Debug.LogFormat("Collision {0} {1}&{2} {3}=={4}", other, other.gameObject.layer, filterLayers.value, filterTag,  other.gameObject.tag);
        if ((other.gameObject.layer & filterLayers.value) == 0) return;
        Debug.Log(1);
        if (filterTag != null && filterTag.Length != 0 && !filterTag.Equals(other.gameObject.tag)) return;
        Debug.Log(2);
        animator.SetTrigger(moveHash);
    }

    void OnDestroy()
    {
        trigger.OnTriggerEnterEvent -= TiggerEnter;
        trigger.OnTriggerExitEvent -= TiggerExit;
    }

    private void TiggerExit(Collider other)
    {
    }
}
