using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LifeBar : MonoBehaviour {

    [SerializeField]
    private RectTransform lifeBarCanvas;

    private float value = 1;

    public void setValue(float value)
    {
        this.value = value;
    }

    // Use this for initialization
    void Start () {}
	
	// Update is called once per frame
	void Update () {
        Vector2 position = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        lifeBarCanvas.position = position;

        Vector3 localScale = lifeBarCanvas.localScale;
        localScale.x = value;
        lifeBarCanvas.localScale = localScale;

        lifeBarCanvas.gameObject.SetActive(value < 1);
    }
}
