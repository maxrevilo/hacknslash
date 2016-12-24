using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class ComboDisplay : MonoBehaviour {

    public ComboManager comboManager;
    CanvasRenderer canvasRenderer;
    public Image ComboTimer;

    public Text comboNumberTxt;

    public float timeToTurnOffAfterFinish = 2f;
    public float timeToShrinkAfterNewHit = .4f;
    public float sizeIncreaseOnNewHit = 1.5f;
    public Color newHitColor = Color.green;
    public Color comboFinishedColor = Color.red;

    int originalTextSize;
    Color originalTextColor;
    float timeToShrink;
    float timeToTurnOff;

    Text[] textNodes;

    // Use this for initialization
    void Awake () {
        if (comboManager == null) throw new Exception("comboManager not set");
        if (comboNumberTxt == null) throw new Exception("comboNumberTxt not set");
        if (ComboTimer == null) throw new Exception("ComboTimer not set");

        canvasRenderer = GetComponent<CanvasRenderer>();

        comboManager.OnComboIncreasedEvent += comboUpdated;
        comboManager.OnComboFinishedEvent += comboFinished;

        originalTextSize = comboNumberTxt.fontSize;
        originalTextColor = comboNumberTxt.color;

        textNodes = GetComponentsInChildren<Text>();
    }

    void Start()
    {
        comboNumberTxt.text = "0";
        setAlpha(0);
    }

    void comboUpdated(float comboDamage)
    {
        setAlpha(1);
        timeToTurnOff = -1;
        timeToShrink = Time.time + timeToShrinkAfterNewHit;
        comboNumberTxt.fontSize = (int) (sizeIncreaseOnNewHit * originalTextSize);
        comboNumberTxt.color = newHitColor;
        comboNumberTxt.text = comboDamage.ToString();
    }

    void comboFinished(float comboDamage)
    {
        timeToTurnOff = Time.time + timeToTurnOffAfterFinish;
        comboNumberTxt.color = comboFinishedColor;
    }

    void Update()
    {
        ComboTimer.fillAmount = comboManager.timeFractionLeft();

        if (timeToShrink> 0 && Time.time > timeToShrink)
        {
            timeToShrink = -1;
            comboNumberTxt.fontSize = originalTextSize;
            comboNumberTxt.color = originalTextColor;
        }

        if(timeToTurnOff > 0 && Time.time > timeToTurnOff)
        {
            timeToTurnOff = -1;
            comboNumberTxt.color = originalTextColor;
            setAlpha(0);

        }
    }

    void setAlpha(float alpha)
    {
        foreach(Text textNode in textNodes)
        {
            textNode.CrossFadeAlpha(alpha, .5f, false);
        }
    }
}
