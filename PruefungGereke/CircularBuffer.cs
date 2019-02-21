using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/// <summary>
/// An implementation of a <see href="https://en.wikipedia.org/wiki/Circular_buffer">CircularBuffer</see>. 
/// </summary>
/// <typeparam name="T">The object that was consumed and is now passed to the delegates function</typeparam>
class CircularBuffer<T> : ICircularBuffer<T> {
    #region
    /// <summary>
    /// Event is signaled when the buffer is not empty
    /// </summary>
    private static ManualResetEvent isNotEmpty = new ManualResetEvent(false);

    /// <summary>
    /// Event is signaled when the buffer is not full
    /// </summary>
    private static ManualResetEvent isNotFull = new ManualResetEvent(true);

    /// <summary>
    /// Stores the data added to the buffer
    /// </summary>
    private volatile T[] buffer;

    /// <summary>
    /// The index at which the Produce functions will add the next entry
    /// </summary>
    private BufferIndex writeIndex;

    /// <summary>
    /// The index at which the Consume functions will read and remove the next entry
    /// </summary>
    private BufferIndex readIndex;

    /// <summary>
    /// Is locked if item is added or removed to ensure an atomic add/remove.
    /// </summary>
    private Mutex operating = new Mutex();

    /// <summary>
    /// The count of entrys in the buffer
    /// </summary>
    private volatile int count;

    /// <summary>
    /// The capacity of this buffer set in constructor
    /// </summary>
    private readonly int capacity;

    /// <summary>
    /// The capacity of this buffer
    /// </summary>
    public int Capacity { get => capacity; }

    /// <summary>
    /// The count of entrys in the buffer
    /// </summary>
    public int Count { get => count; }

    /// <summary>
    /// Is true if the buffer doesn't contain any items
    /// </summary>
    public bool IsEmpty => (0 == count);

    /// <summary>
    /// Is true if the buffer is full
    /// </summary>
    public bool IsFull => (count == capacity);
    #endregion
    /// <summary>
    /// Creates a CircularBuffer of the given type
    /// </summary>
    /// <param name="capacity">The capacity of the buffer</param>
    public CircularBuffer(int capacity) {
        buffer = new T[capacity];
        this.capacity = capacity;
        writeIndex = new BufferIndex(capacity); // Indices created at 0 (Automatic overlow at capacity)
        readIndex = new BufferIndex(capacity);
    }

    /// <summary>
    /// Clears the buffer
    /// </summary>
    public void Clear() {
        count = 0; // Clears the buffer through resetting the count + indices
        writeIndex.Index = 0;
        readIndex.Index = 0;
    }

    /// <summary>
    /// Consumes the oldest item in the buffer
    /// </summary>
    /// <returns>Returns consumed item</returns>
    public T Consume() {
        if (IsEmpty) { // Can't consume if the array is empty
            throw new BufferUnderflowException();
        }

        T ret;
        Interlocked.Decrement(ref count); // Decrements the buffers count and increases the readIndex
        lock(readIndex){
            ret = buffer[readIndex.Index++];
        }

        return ret;
    }

    /// <summary>
    /// Consumes all items in the buffer and calls the given function
    /// </summary>
    /// <param name="callback">This function will be called when an item is consumed. First parameter will be the item itself.</param>
    public void ConsumeAll(Callback<T> callback) {
        for (; ; ) {
            try {
                callback(Consume()); // Call given function with consumed item as parameter
            } catch (BufferUnderflowException) {
                return;
            }
        }
    }

    /// <summary>
    /// Adds an object to the buffer
    /// </summary>
    /// <param name="obj">The object to add to the buffer</param>
    public void Produce(T obj) {
        if (IsFull) {// Can't produce if the array is full
            throw new BufferOverflowException();
        }

        Interlocked.Increment(ref count); // Increments the buffers count and increases the writeIndex
        lock (writeIndex){
            buffer[writeIndex.Index++] = obj;
        }
    }

    /// <summary>
    /// Adds multiple objects to the buffer
    /// </summary>
    /// <param name="add">The objects to add to the buffer</param>
    /// <returns>The count of added items</returns>
    public int ProduceAll(IEnumerable<T> add) {
        var tmpCount = 0;
        foreach (var item in add) {
            try {
                Produce(item); // Call produce for every item in add
            } catch (BufferOverflowException) {
                return tmpCount;
            }

            tmpCount++;
        }

        return tmpCount;
    }

    /// <summary>
    /// This function blocks until an item could be consumed
    /// </summary>
    /// <returns>Returns the consumed item</returns>
    public T WaitConsume() {
        if (IsEmpty) { // If buffer is empty wait for production of an item
            isNotEmpty.Reset();
            isNotEmpty.WaitOne();
        }

        operating.WaitOne(); // Lock mutex to ensure consumption and setting of ManualRelaseEvent occur without interference from WaitProduce
        var ret = Consume();
        isNotFull.Set();
        operating.ReleaseMutex();
        return ret;
    }

    /// <summary>
    /// This function blocks until the item could be added
    /// </summary>
    /// <param name="obj">The object to add to the buffer</param>
    public void WaitProduce(T obj) {
        if (IsFull) { // If buffer is full wait for consumption of an item
            isNotFull.Reset();
            isNotFull.WaitOne();
        }

        operating.WaitOne(); // Lock mutex to ensure production and setting of ManualRelaseEvent occur without interference from WaitConsume
        Produce(obj);
        isNotEmpty.Set();
        operating.ReleaseMutex();
    }
}
