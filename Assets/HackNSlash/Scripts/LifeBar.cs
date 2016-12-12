using System;
using UnityEngine;

public class LifeBar : MonoBehaviour {
    [SerializeField]
    private Vector3 worldDisplacement = new Vector3(0, 2f, 0);

    [SerializeField]
    private RectTransform lifeBarContainer;

    [SerializeField]
    private RectTransform canvas;
    
    [SerializeField]
    private RectTransform lifeFill;

    public PlayerConstitution playerConstitution;
    
    [SerializeField]
    private float lifeBarChangeSpeed = 0.1f;

    private float lifeFraction = 1;

    void Start () {
        if(lifeBarContainer == null) {
            throw new Exception("lifeBarCanvas not set");
        }
        if(lifeFill == null) {
            throw new Exception("lifeFill not set");
        }
        if(playerConstitution == null) {
            throw new Exception("playerConstitution not set");
        }
        
        playerConstitution.OnLifeChangeEventEvent += LifeChangeEvent;
        playerConstitution.OnDieEvent += DieEvent;
    }
    
    void LifeChangeEvent(PlayerMain playerMain, float current, float previous) {
        if(current > 0) canvas.gameObject.SetActive(true);

        lifeFraction = current / playerConstitution.defHitPoints;
    }
    
    void DieEvent(PlayerMain playerMain, float lastHit) {
        canvas.gameObject.SetActive(false);
    }

	void Update () {
        UpdatePosition();
        UpdateLifeFillScale();
    }

    void UpdatePosition() {
        Vector3 position = Camera.main.WorldToScreenPoint(
            playerConstitution.transform.position + worldDisplacement
        );

        lifeBarContainer.anchoredPosition = new Vector2(
            position.x / Screen.width * canvas.sizeDelta.x,
            position.y / Screen.height * canvas.sizeDelta.y
        );
    }

    void UpdateLifeFillScale() {
        Vector3 localScale = lifeFill.localScale;
        localScale.x += (lifeFraction - localScale.x) * lifeBarChangeSpeed;
        lifeFill.localScale = localScale;

        if(lifeFraction > 0) {
            canvas.gameObject.SetActive(lifeFraction < 1f);
        }
    }
}
