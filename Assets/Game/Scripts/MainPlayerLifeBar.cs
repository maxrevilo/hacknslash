using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MainPlayerLifeBar : MonoBehaviour {

    public PlayerConstitution playerConstitution;

    private RectTransform reactTransform;

    // Use this for initialization
    void Awake () {
        reactTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 localScale = reactTransform.localScale;
        localScale.x = playerConstitution.LifeFraction();
        reactTransform.localScale = localScale;
    }
}
