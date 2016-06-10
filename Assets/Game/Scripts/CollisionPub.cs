using UnityEngine;

public class CollisionPub : MonoBehaviour {

    public delegate void TriggerEnter(Collider other);
    public event TriggerEnter OnTriggerEnterEvent;

    public delegate void TriggerExit(Collider other);
    public event TriggerExit OnTriggerExitEvent;

	void OnTriggerEnter(Collider other)
    {
		if(OnTriggerEnterEvent != null) OnTriggerEnterEvent(other);
	}

	void OnTriggerExit(Collider other)
    {
		if(OnTriggerExitEvent != null) OnTriggerExitEvent(other);
	}
}
