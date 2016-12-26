namespace DarkWinter.Util.DataStructures
{
    public delegate void DisposedEvent(Poolable instance);

    /// <summary>
    /// Defines a class that can be used in an ObjectPool<Poolable>
    /// </summary>
    public interface Poolable {
        
        event DisposedEvent OnDisposed;
        
        /// <summary>
        /// Called when the instance is retreived from the pool to be used. 
        /// Used to define setup logic for the instance.
        /// </summary>
        void Setup();

        /// <summary>
        /// Called when the instance is taken back to the pool.
        /// Used to run cleanup logic.
        /// </summary>
        void Reset();
    }
}
