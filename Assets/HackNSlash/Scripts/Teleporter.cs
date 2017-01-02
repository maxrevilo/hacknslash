using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : Resetable {

	[SerializeField]
	private string playerTag = "Player";

	[SerializeField]
	private CollisionPub trigger;

	[SerializeField]
	private Transform destiny;

	protected override void Awake() {
		base.Awake();
		if(trigger == null) throw new Exception("trigger not set");
		if(destiny == null) throw new Exception("destiny not set");
	}

    protected override void _Reset()
    {
        trigger.OnTriggerEnterEvent += OnTriggerEnter;
    }

	private void OnTriggerEnter(Collider other) {
		if(other.gameObject.tag.EndsWith(playerTag)) {
			other.transform.position = destiny.position;
			other.transform.rotation = destiny.rotation;
		}
	}
}
