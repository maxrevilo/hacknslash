using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class ComboDisplay : MonoBehaviour {

    public ComboManager comboManager;
    RectTransform rectTransform;
    public Image ComboTimer;

    public Text comboNumberTxt;

    public float timeToTurnOffAfterFinish = 2f;
    public float timeToShrinkAfterNewHit = .4f;
    public float sizeIncreaseOnNewHit = 1.5f;
    public Color originalColor = Color.red;
    public Color newHitColor = Color.green;
    public Color comboFinishedColor = Color.white;

    int originalTextSize;
    float timeToShrink;
    float timeToTurnOff;

    Graphic[] UINodes;

    // Use this for initialization
    void Awake () {
        if (comboManager == null) throw new Exception("comboManager not set");
        if (comboNumberTxt == null) throw new Exception("comboNumberTxt not set");
        if (ComboTimer == null) throw new Exception("ComboTimer not set");

        rectTransform = GetComponent<RectTransform>();

        comboManager.OnComboIncreasedEvent += comboUpdated;
        comboManager.OnComboFinishedEvent += comboFinished;

        originalTextSize = comboNumberTxt.fontSize;

        UINodes = GetComponentsInChildren<Graphic>();
    }

    protected void OnDestroy() {
        comboManager.OnComboIncreasedEvent -= comboUpdated;
        comboManager.OnComboFinishedEvent -= comboFinished;
    }

    void Start()
    {
        comboNumberTxt.text = "0";
        setColor(comboFinishedColor, 0);
        setAlpha(0f, 0);
    }

    void comboUpdated(float comboDamage)
    {
        setAlpha(1);
        timeToTurnOff = -1;
        timeToShrink = Time.time + timeToShrinkAfterNewHit;
        //comboNumberTxt.fontSize = (int) (sizeIncreaseOnNewHit * originalTextSize);
        rectTransform.localScale = Vector3.one * sizeIncreaseOnNewHit;
        setColor(newHitColor, timeToShrinkAfterNewHit / 4f);
        comboNumberTxt.text = comboDamage.ToString();
    }

    void comboFinished(float comboDamage)
    {
        timeToTurnOff = Time.time + timeToTurnOffAfterFinish;
        setColor(comboFinishedColor);
    }

    void Update()
    {
        ComboTimer.fillAmount = comboManager.timeFractionLeft();

        if (timeToShrink> 0 && Time.time > timeToShrink)
        {
            timeToShrink = -1;
            //comboNumberTxt.fontSize = originalTextSize;
            rectTransform.localScale = Vector3.one;
            setColor(originalColor, .5f);
        }

        if(timeToTurnOff > 0 && Time.time > timeToTurnOff)
        {
            timeToTurnOff = -1;
            setColor(originalColor);
            setAlpha(0);

        }
    }

    void setAlpha(float alpha, float time = .5f)
    {
        foreach(Graphic uiNode in UINodes)
        {
            uiNode.CrossFadeAlpha(alpha, time, false);
        }
    }


    void setColor(Color color, float time = .2f)
    {
        foreach (Graphic uiNode in UINodes)
        {
            uiNode.CrossFadeColor(color, time, false, true);
        }
    }
}
