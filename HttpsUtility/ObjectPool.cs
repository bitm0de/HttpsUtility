using System;
using Crestron.SimplSharp;

namespace HttpsUtility
{
    /// <summary>
    /// An object pool to help with minimizing GC cleanup and re-allocations.
    /// </summary>
    /// <typeparam name="T">Pool object type.</typeparam>
    public sealed class ObjectPool<T> : IDisposable
        where T : class, new()
    {
        private readonly  CCriticalSection _disposeLock = new CCriticalSection();
        private readonly CrestronQueue<T> _objectPool;

        private readonly CEvent _queueAddEvent = new CEvent(false, true);
        private readonly CEvent _queueReturnEvent = new CEvent(false, true);
        private int _currentCount;
        private bool _disposed;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new object pool.
        /// </summary>
        public ObjectPool()
            : this(64) { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new object pool with a specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">Initial pool capacity.</param>
        public ObjectPool(int initialCapacity)
            : this(initialCapacity, -1, () => new T()) { }

        /// <summary>
        /// Initializes a new object pool with a specified initial and max capacity.
        /// </summary>
        /// <param name="initialCapacity">Initial pool capacity.</param>
        /// <param name="maxCapacity">Max pool capacity</param>
        /// <param name="initFunc">Initialization function</param>
        /// <exception cref="ArgumentException">Invalid initial or max capacity.</exception>
        public ObjectPool(int initialCapacity, int maxCapacity, Func<T> initFunc)
        {
            if (initialCapacity < 1)
                throw new ArgumentException("Initial capacity cannot be less than 1.");

            if (maxCapacity < 1)
                throw new ArgumentException("Max capacity cannot be less than 1.");

            if (initialCapacity > maxCapacity)
                throw new ArgumentException("Initial capacity cannot be greater than max capacity.");

            MaxCapacity = maxCapacity;
            _objectPool = new CrestronQueue<T>(initialCapacity);

            AddToPool(initialCapacity, initFunc);
        }

        /// <summary>
        /// Max capacity of the object pool.
        /// </summary>
        /// <remarks>A max capacity of -1 indicates there is no max capacity.</remarks>
        public int MaxCapacity { get; private set; }

        /// <summary>
        /// Determines whether internal objects in the queue are disposed
        /// when this object is disposed if this pool is holding IDisposable objects.
        /// </summary>
        public bool CleanupPoolOnDispose { get; set; }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Adds (or returns) an object to the pool.
        /// </summary>
        /// <param name="obj"></param>
        public void AddToPool(T obj)
        {
            if (Interlocked.Increment(ref _currentCount) > MaxCapacity)
                _queueReturnEvent.Wait();

            if (_disposed) return;
            _objectPool.Enqueue(obj);
            _queueAddEvent.Set();
        }

        /// <summary>
        /// Adds a new object to the pool.
        /// </summary>
        /// <param name="initFunc">Initialization function</param>
        /// <returns>True if object could be added to the pool; otherwise false.</returns>
        public bool AddToPool(Func<T> initFunc)
        {
            return AddToPool(1, initFunc);
        }

        /// <summary>
        /// Adds multiple objects to the pool.
        /// </summary>
        /// <param name="count">Number of instantiated objects to add.</param>
        /// <param name="initFunc">Initialization function</param>
        /// <returns>True if objects could be added to the pool; otherwise false.</returns>
        public bool AddToPool(int count, Func<T> initFunc)
        {
            for (var i = 0; i < count; i++)
            {
                if (_disposed || _currentCount == MaxCapacity)
                    return false;

                Interlocked.Increment(ref _currentCount);
                _objectPool.Enqueue(initFunc.Invoke());
                _queueAddEvent.Set();
            }

            return true;
        }

        /// <summary>
        /// Retrieves an object from the pool.
        /// </summary>
        /// <returns>Pool object.</returns>
        public T GetFromPool()
        {
            if (_currentCount == 0)
                _queueAddEvent.Wait();

            if (_disposed) return null;
            Interlocked.Decrement(ref _currentCount);
            var obj = _objectPool.Dequeue();
            _queueReturnEvent.Set();
            return obj;
        }

        private void Dispose(bool disposing)
        {
            _disposed = true;
            if (disposing)
            {
                _queueAddEvent.Set();
                _queueReturnEvent.Set();

                if (CleanupPoolOnDispose && typeof(IDisposable).IsAssignableFrom(typeof(T)))
                    while (!_objectPool.IsEmpty)
                        ((IDisposable)_objectPool.Dequeue()).Dispose();

                _objectPool.Dispose();
                _queueAddEvent.Dispose();
                _queueReturnEvent.Dispose();
                _disposeLock.Dispose();
            }
        }
    }
}