using System;
using UnityEngine;

[ExecuteInEditMode]
public class LifeBar : MonoBehaviour {
    [SerializeField]
    private Vector3 worldDisplacement = new Vector3(0, 2f, 0);

    [SerializeField]
    private RectTransform lifeBarCanvas;
    
    [SerializeField]
    private RectTransform lifeFill;

    public PlayerConstitution playerConstitution;
    
    [SerializeField]
    private float lifeBarChangeSpeed = 0.1f;

    private float lifeFraction = 1;

    void Start () {
        if(lifeBarCanvas == null) {
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
        if(current > 0) lifeBarCanvas.gameObject.SetActive(true);

        lifeFraction = current / playerConstitution.defHitPoints;
    }
    
    void DieEvent(PlayerMain playerMain, float lastHit) {
        lifeBarCanvas.gameObject.SetActive(false);
    }

	void Update () {
        Vector2 position = RectTransformUtility.WorldToScreenPoint(
            Camera.main,
            playerConstitution.transform.position + worldDisplacement
        );
        lifeBarCanvas.position = position;
        transform.position = position;

        Vector3 localScale = lifeFill.localScale;
        localScale.x += (lifeFraction - localScale.x) * lifeBarChangeSpeed;
        lifeFill.localScale = localScale;

        if(lifeFraction > 0) {
            lifeBarCanvas.gameObject.SetActive(lifeFraction < 1f);
        }
    }
}
