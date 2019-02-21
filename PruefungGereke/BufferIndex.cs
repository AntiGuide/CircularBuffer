using System;

/// <summary>
/// A class to help looping through the CircularBuffer
/// </summary>
class BufferIndex {
    /// <summary>
    /// The capacity of the corresponding buffer
    /// </summary>
    private int capacity;

    /// <summary>
    /// The current index
    /// </summary>
    private volatile int index;

    /// <summary>
    /// The current index
    /// </summary>
    public int Index {
        get {
            return index;
        }

        set {
            var newValue = value;
            while (newValue >= capacity) { // Decrease/Increase value until it is between 0 and capacity
                newValue -= capacity;
            }

            while (newValue < 0) {
                newValue += capacity;
            }

            index = newValue;
        }
    }

    /// <summary>
    /// Creates a BufferIndex
    /// </summary>
    /// <param name="capacity">The capacity of the corresponding buffer</param>
    public BufferIndex(int capacity) {
        if (capacity <= 0) { // Capacity has to be greater that 0
            throw new ArgumentException("Capacity for a BufferIndex must me greater that 0!");
        }

        this.capacity = capacity;
    }
}
