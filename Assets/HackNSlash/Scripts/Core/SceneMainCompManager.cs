using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    public class SceneMainCompManager
    {
        private static SceneMainCompManager instance;

        private static void Init()
        {
            if (instance == null)
            {
                instance = new SceneMainCompManager();
            }
        }

        public static SceneMainCompManager Inst()
        {
            Init();
            return instance;
        }

        public LinkedList<SceneMainComp> activeScenes { get; private set; }

        private SceneMainCompManager()
        {
            activeScenes = new LinkedList<SceneMainComp>();
        }

        public void AddToActiveScenes(SceneMainComp sceneMainComp)
        {
            Init();
            activeScenes.AddLast(sceneMainComp);
        }

        public void RemoveFromActiveScenes(SceneMainComp sceneMainComp)
        {
            Init();
            activeScenes.Remove(sceneMainComp);
        }

        public Component GetComponentInActiveScenes(System.Type type, bool includeInactive = false)
        {
            foreach (SceneMainComp sceneMainComp in activeScenes)
            {
                Component comp = sceneMainComp.GetComponentInChildren(type, includeInactive);
                if (comp != null) return comp;
            }
            return null;
        }
    }
}
