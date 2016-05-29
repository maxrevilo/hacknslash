using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class MainPlayerLifeBar : MonoBehaviour {

    public PlayerMain player;

    private RectTransform reactTransform;

    // Use this for initialization
    void Awake () {
        reactTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 localScale = reactTransform.localScale;
        localScale.x = player.LifeFraction();
        reactTransform.localScale = localScale;
    }
}
