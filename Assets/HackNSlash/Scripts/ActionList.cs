using System;
using System.Collections.Generic;

namespace DarkWinter.Util.DataStructures
{
    public class ActionList
    {
        public virtual void Update(float deltaTime)
        {
            uint blockedLanes = 0;
            LinkedListNode<LAction> node = actions.First;
            while(node != null)
            {
                LAction action = node.Value;

                if ((blockedLanes & action.lanes) != 0)
                    continue;

                action.Update(deltaTime);
                if (action.isBlocking)
                    lanes |= action.lanes;

                if (action.isFinished)
                {
                    action.OnEnd();
                    Remove(action);
                }

                node = node.Next;
            }
        }

        public void PushFront(LAction action)
        {
            actions.AddFirst(action);
            action.ownerList = this;
        }

        public void PushBack(LAction action)
        {
            actions.AddLast(action);
            action.ownerList = this;
        }

        public bool Remove(LAction action)
        {
            bool removed = actions.Remove(action);
            if (removed)
            {
                action.ownerList = null;
            }
            return removed;
        }

        public void Clear()
        {
            actions.Clear();
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
                return true;
            }
            else return false;
        }

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
        private uint lanes;
        // private float duration;
        // private float timeElapsed;
        // private float percentDone;
        // private bool blocking;
    }
}
