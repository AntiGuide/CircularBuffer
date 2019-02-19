using System;

class BufferIndex {
    private int capacity;
    private volatile int index;
    public int Index {
        get {
            return index;
        }

        set {
            var newValue = value;
            while (newValue >= capacity) {
                newValue -= capacity;
            }

            while (newValue < 0) {
                newValue += capacity;
            }

            index = newValue;
        }
    }

    public BufferIndex(int capacity) {
        if (capacity <= 0) {
            throw new ArgumentException("Capacity for a BufferIndex must me greater that 0!");
        }

        this.capacity = capacity;
    }
}
