using System;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(RectTransform))]
public class SkillResourceBar : MonoBehaviour {

    public PlayerSkillResource playerSkillResource;
    private RectTransform reactTransform;
    
    void Awake ()
    {
        Assert.IsNotNull(playerSkillResource, "playerConstitution not set");
        reactTransform = GetComponent<RectTransform>();
    }

	void Update () {
        Vector3 localScale = reactTransform.localScale;
        localScale.x = playerSkillResource.GetFractionAmount();
        reactTransform.localScale = localScale;
    }
}
