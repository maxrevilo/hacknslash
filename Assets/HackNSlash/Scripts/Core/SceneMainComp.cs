using UnityEngine;

namespace Game.Core
{
    public class SceneMainComp: BaseComponent
    {
        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 120;
            SceneMainCompManager.Inst().AddToActiveScenes(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneMainCompManager.Inst().RemoveFromActiveScenes(this);
        }
    }
}
