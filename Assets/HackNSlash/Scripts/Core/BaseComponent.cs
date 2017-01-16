using UnityEngine;

namespace Game.Core
{
    public class BaseComponent : MonoBehaviour
    {
        protected virtual void Awake() { }
        protected virtual void OnEnable() { }

        protected virtual void Start() { }
        protected virtual void OnApplicationPause() { }

        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void LateUpdate() { }

        protected virtual void OnDisable() { }
        protected virtual void OnApplicationQuit() { }
        protected virtual void OnDestroy() { }

        protected virtual void DontDestroyOnLoad()
        {
            Object.DontDestroyOnLoad(gameObject);
        }
    }
}