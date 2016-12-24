using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkWinter.Util.DataStructures
{
    public abstract class LAction
    {
        public abstract void Update(float deltaTime);
        public abstract void OnStart();
        public abstract void OnEnd();

        public bool isFinished = false;
        public bool isBlocking = false;
        public uint lanes = 0;
        public float elapsed = 0;
        public float duration = 0;

        public ActionList ownerList;

        public void InsertInFrontOfMe(LAction action)
        {
            ownerList.InsertInFront(this, action);
        }
    }
}
