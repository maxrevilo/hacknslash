using System;
using System.Collections.Generic;

namespace DarkWinter.Util.DataStructures
{
    /// <summary>
    /// Implements a generic pool of objects
    /// </summary>
    public class ObjectsPool<T> where T : Poolable {

        private Stack<T> available;
        private Func<T> factoryMethod;
        private List<T> pooled;

        public int Size() {
            return pooled.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        public ObjectsPool(Func<T> factoryMethod, int initialCapacity = 10) {
            this.factoryMethod = factoryMethod;
            available = new Stack<T>(initialCapacity);
            pooled = new List<T>(initialCapacity);

            for(int i = 0; i < initialCapacity; i++) {
                generateInstance();
            }
        }

        /// <summary>
        /// Returns an object from the pool to be used, Setup is already called.
        /// </summary>
        public T retreive() {
            if(available.Count == 0) {
                generateInstance();
            }
            T instance = available.Pop();
            instance.OnDisposed += DisposeInstance;
            instance.Setup();
            return instance;
        }

        private T generateInstance() {
            T instance = factoryMethod();
            instance.Reset();
            available.Push(instance);
            pooled.Add(instance);
            return instance;
        }

        private void DisposeInstance(Poolable instance) {
            instance.Reset();
            available.Push((T) instance);
            instance.OnDisposed -= DisposeInstance;
        }
    }
}
