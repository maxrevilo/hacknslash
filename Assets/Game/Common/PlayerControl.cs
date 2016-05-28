using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class PlayerControl : MonoBehaviour {
	private PlayerMain playerMain;

	private Camera mainCamera;

	void Awake() {
		playerMain = GetComponent<PlayerMain>();
		mainCamera = Camera.main;
	}

	void Start () {
	}
	
	void FixedUpdate () {
		if(Input.GetMouseButtonDown(0)) {
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = 1;
			Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
			Ray mouseRay = new Ray(worldPosition, mainCamera.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(mouseRay, out hit))
			{
				playerMain.Attack(hit.point);
			} else {
				Debug.LogError("Failed projection of the mouse");
			}
		}
	}
}
