using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMain : MonoBehaviour {
	public int team = 1;

	void Awake() {
		if (atackArea == null) throw new Exception("<atackArea> not set");
		atackArea.gameObject.SetActive(false);
	}

	void Start () {
	#region Atack
		currentAttackCooldown = 0;
	#endregion
	#region Motion
		playerRigidBody = GetComponent<Rigidbody>();
	#endregion
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void FixedUpdate () {
	#region Atack
		currentAttackCooldown -= Time.fixedDeltaTime;
	#endregion
	#region Motion
		if(advancing) {
			playerRigidBody.MovePosition(
				transform.position + defSpeed * transform.forward
			);
		}
	#endregion
	}

#region Motion
	[SerializeField]
	private float defSpeed = 0.3f;
	private bool advancing;
	private Rigidbody playerRigidBody;

	public void Advance() {
		advancing = true;
	}
	public void Stop() {
		advancing = false;
	}
	
	public void LookAt(Vector3 position) {
		Vector3 forward = position - transform.position;
		forward.y = 0;
		playerRigidBody.MoveRotation(
			Quaternion.LookRotation(forward, transform.up)
		);
	}
#endregion

#region Atack
	public Transform atackArea;

	[SerializeField]
	private float defAttackCooldown = 1;

	private float currentAttackCooldown;

	public void Attack(Vector3 position) {
		if(currentAttackCooldown <= 0) {
			//Debug.DrawLine(transform.position, position, Color.red, 0.05f);
			currentAttackCooldown = defAttackCooldown;
			LookAt(position);
			StartCoroutine(ActivateAtackArea());
		}
	}

	private IEnumerator ActivateAtackArea() {
		atackArea.gameObject.SetActive(true);
		yield return new WaitForEndOfFrame();
		atackArea.gameObject.SetActive(false);
	}
#endregion
}
