using UnityEngine;
using System;

public class CameraRail : MonoBehaviour {

	[SerializeField]
	private PlayerMain target;
	
	[SerializeField]
	private float minSpeed = 0f;
	[SerializeField]
	private float speedFactor = 0.025f;
	
	private Vector3 basePosition;

	// Use this for initialization
	void Awake () {
		if(target == null) throw new Exception("target not set");
		basePosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		float distanceSqr = Vector3.SqrMagnitude(transform.position - target.transform.position);
		float speed = Mathf.Max(minSpeed, distanceSqr * speedFactor);

		basePosition = Vector3.MoveTowards(basePosition, target.transform.position, speed);

		transform.position = basePosition;
	}
}
