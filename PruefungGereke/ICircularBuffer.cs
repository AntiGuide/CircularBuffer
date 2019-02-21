using System.Collections.Generic;

/// <summary>
/// Callback delegate for Consume all
/// </summary>
/// <param name="obj">The object that was consumed and is now passed to the delegates function</param>
public delegate void Callback<T>(T obj);

/// <summary>
/// The interface for CircularBuffers
/// </summary>
/// <typeparam name="T">The type of all the elements stored in the buffer</typeparam>
internal interface ICircularBuffer<T> {
    /// <summary>
    /// The capacity of the buffer
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// The count of item in the buffer
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Is true if the buffer doesn't contain any items
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Is true if the buffer is full
    /// </summary>
    bool IsFull { get; }

    /// <summary>
    /// Adds an object to the buffer
    /// </summary>
    /// <param name="obj">The object to add to the buffer</param>
    void Produce(T obj);

    /// <summary>
    /// Consumes the oldest item in the buffer
    /// </summary>
    /// <returns>Returns consumed item</returns>
    T Consume();

    /// <summary>
    /// Clears the buffer
    /// </summary>
    void Clear();

    /// <summary>
    /// Adds multiple objects to the buffer
    /// </summary>
    /// <param name="add">The objects to add to the buffer</param>
    /// <returns>The count of added items</returns>
    int ProduceAll(IEnumerable<T> add);

    /// <summary>
    /// Consumes all items in the buffer and calls the given function
    /// </summary>
    /// <param name="callback">This function will be called when an item is consumed. First parameter will be the item itself.</param>
    void ConsumeAll(Callback<T> callback);

    /// <summary>
    /// This function blocks until the item could be added
    /// </summary>
    /// <param name="obj">The object to add to the buffer</param>
    void WaitProduce(T obj);

    /// <summary>
    /// This function blocks until an item could be consumed
    /// </summary>
    /// <returns>Returns the consumed item</returns>
    T WaitConsume();
}