using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryUIPanel : MonoBehaviour {
    private BattleGameScene scene;
    Graphic[] UINodes;

    void Awake()
    {
        UINodes = GetComponentsInChildren<Graphic>();
    }

    void Start () {
        scene = GetComponentInParent<BattleGameScene>();
        if (scene == null) throw new Exception("BattleGameScene not found in parents");

        scene.OnVictoryEvent += Victory;
        setAlpha(0f, 0);
    }

    void OnDestroy()
    {
        scene.OnGameOverEvent -= Victory;
    }

    void Victory()
    {
        foreach (Graphic uiNode in UINodes)
        {
            uiNode.enabled = true;
        }
        setAlpha(1f, 3f);
    }

    void setAlpha(float alpha, float time = .5f)
    {
        foreach (Graphic uiNode in UINodes)
        {
            uiNode.CrossFadeAlpha(alpha, time, false);
        }
    }
}
