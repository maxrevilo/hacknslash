using System;
using UnityEngine;
using System.Collections;


public class AddHitPointsOnContact : MonoBehaviour {

	[SerializeField]
	private CollisionPub collisionPub;

	[SerializeField]
	private float damage = 0;

	[SerializeField]
	private bool destroyOnContact = false;

	void Awake() {
		if(collisionPub == null) throw new Exception("collisionPub not defined");

		collisionPub.OnTriggerEnterEvent += TriggerEnter;
	}

	void TriggerEnter(Collider other) {
		PlayerConstitution player = other.GetComponent<PlayerConstitution>();
		if(player != null)  {
			player.AddHitPoints(damage);
			if(destroyOnContact) gameObject.DestroyAPS();
		}
	}
}
