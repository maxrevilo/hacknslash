using Game.Core;
using UnityEngine;
using System.Collections;

public class CollisionPub : BaseComponent {

    public ArrayList collisions { get; protected set; }
    public bool holdRecordOfCollisions = false;

    public delegate void TriggerEnter(Collider other);
    public event TriggerEnter OnTriggerEnterEvent;

    public delegate void TriggerExit(Collider other);
    public event TriggerExit OnTriggerExitEvent;

    protected override void Awake()
    {
        base.Awake();
        collisions = new ArrayList();
    }

    void OnTriggerEnter(Collider other)
    {
		if(OnTriggerEnterEvent != null) OnTriggerEnterEvent(other);
	}

	void OnTriggerExit(Collider other)
    {
		if(OnTriggerExitEvent != null) OnTriggerExitEvent(other);
	}
}
