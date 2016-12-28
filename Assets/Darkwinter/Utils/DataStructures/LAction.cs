namespace DarkWinter.Util.DataStructures
{
    public abstract class LAction: Poolable
    {
        public event DisposedEvent OnDisposed;

        public virtual void Update(float deltaTime) {
            isBlocked = false;
        }

        public void OnStart()
        {
            isFinished = false;
            elapsed = 0;
            _OnStart();
        }

        public abstract void _OnStart();

        public void OnEnd() {
            if(OnDisposed != null) OnDisposed(this);
            ownerList = null;

            _OnEnd();
        }

        protected abstract void _OnEnd();

        public void EarlyUpdate() {
            isBlocked = true;
        }
        public void LateUpdate() {}

        public bool isFinished = false;
        public bool isBlocking = false;
        public uint lanes = 0;
        public float elapsed = 0;
        public float duration = 0;

        public ActionList ownerList;

        public bool isBlocked { get; private set; }

        public void InsertInFrontOfMe(LAction action)
        {
            ownerList.InsertInFront(this, action);
        }

        public void Setup() {}
        public void Reset() {}
    }
}
