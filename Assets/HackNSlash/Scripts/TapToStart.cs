using System;
using UnityEngine;
using System.Collections;

public class TapToStart : MonoBehaviour {
	
	public GameObject infoText; 

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;
		if(infoText == null) {
			throw new Exception("infoText not set.");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.anyKeyDown) {
			infoText.SetActive(false);
			Time.timeScale = 1;
			this.enabled = false;
		}
	}
}
