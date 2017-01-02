using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Resetable : MonoBehaviour {

    protected virtual void Awake()
    {

    }

    protected virtual void Start () {
        ResetComponent();
    }

    protected virtual void Update () {

    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void OnDestroy()
    {

    }

    /// <summary>
    /// Resets the component to its intented initial state, usefull for calling after
    /// retreiving from object pools.
    /// </summary>
    public void ResetComponent()
    {
        _Reset();
    }

    /// <summary>
    /// This procedure will reset the component to it's initial inteted state. For example will
    /// reset hit points, stamina, ect.
    /// </summary>
    protected abstract void _Reset();
}
