using System.Collections.Generic;

namespace DarkWinter.Util.DataStructures
{
    public class ActionList
    {

        public ActionList() {
            actions = new LinkedList<LAction>();
        }

        public virtual void Update(float deltaTime)
        {
            foreach (LAction action in actions) action.EarlyUpdate();

            uint blockedLanes = 0;
            LinkedListNode<LAction> node = actions.First;
            while(node != null)
            {
                LinkedListNode<LAction> nextNode = node.Next;
                LAction action = node.Value;

                if ((blockedLanes & action.lanes) != 0) {
                    node = nextNode;
                    continue;
                }

                action.Update(deltaTime);
                if (action.isBlocking) {
                    blockedLanes |= action.lanes;
                }

                if (action.isFinished)
                {
                    Remove(action);
                }

                node = nextNode;
            }

            foreach (LAction action in actions) action.LateUpdate();
        }

        public void PushFront(LAction action)
        {
            actions.AddFirst(action);
            action.ownerList = this;
            action.OnStart();
        }

        public void PushBack(LAction action)
        {
            actions.AddLast(action);
            action.ownerList = this;
            action.OnStart();
        }

        public bool Remove(LAction action)
        {
            bool removed = actions.Remove(action);
            if (removed)
            {
                action.OnEnd();
            }
            return removed;
        }

        public void Clear()
        {
            LinkedListNode<LAction> node = actions.First;
            while(node != null)
            {
                LinkedListNode<LAction> nextNode = node.Next;
                Remove(node.Value);
                node = nextNode;
            }
        }

        public LAction First
        {
            get {
                return actions.First.Value;
            }
        }

        public LAction Last
        {
            get {
                return actions.Last.Value;
            }
        }

        public bool IsEmpty
        {
            get {
                return actions.Count == 0;
            }
        }

        public bool InsertInFront(LAction action, LAction value)
        {
            LinkedListNode<LAction> node = actions.Find(action);
            if (node != null)
            {
                actions.AddBefore(node, value);
                value.ownerList = this;
                return true;
            }
            else return false;
        }

        public LinkedList<LAction> list { get { return actions; } }

        /*public float TimeLeft
        {
            get {
                return 0f;
            }
        }

        public bool IsBlocking
        {
            get
            {
                return false;
            }
        }*/
        
        private LinkedList<LAction> actions;
        // private uint lanes;
        // private float duration;
        // private float timeElapsed;
        // private float percentDone;
        // private bool blocking;
    }
}
