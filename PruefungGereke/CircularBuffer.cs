using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class CircularBuffer<T> : ICircularBuffer<T> {
    private T[] buffer;
    private BufferIndex writePointer;
    private Mutex writePointerMutex = new Mutex();
    private BufferIndex readPointer;
    private Mutex readPointerMutex = new Mutex();
    private int count;
    private int capacity;

    public int Capacity { get => capacity; }
    public int Count { get => count; }
    public bool IsEmpty => (0 == Count);
    public bool IsFull => (Count == Capacity);

    public CircularBuffer(int capacity) {
        buffer = new T[capacity];
        this.capacity = capacity;
        writePointer = new BufferIndex(capacity);
        readPointer = new BufferIndex(capacity);
    }

    public void Clear() {
        count = 0;
        writePointer.Index = 0;
        readPointer.Index = 0;
    }

    public T Consume() {
        if (0 == count) {
            throw new BufferUnderflowException();
        }

        Interlocked.Decrement(ref count);
        readPointerMutex.WaitOne();
        var ret = buffer[readPointer.Index++];
        readPointerMutex.ReleaseMutex();
        return ret;
    }

    public void ConsumeAll(Callback<T> callback) {
        for (; ; ) {
            try {
                callback(Consume());
            } catch (BufferUnderflowException) {
                return;
            }
        }
    }

    public void Produce(T obj) {
        if (Count == Capacity) {
            throw new BufferOverflowException();
        }

        Interlocked.Increment(ref count);
        writePointerMutex.WaitOne();
        buffer[writePointer.Index++] = obj;
        writePointerMutex.ReleaseMutex();
    }

    public int ProduceAll(IEnumerable<T> add) {
        var tmpCount = 0;
        foreach (var item in add) {
            try {
                Produce(item);
            } catch (BufferOverflowException) {
                return tmpCount;
            }

            tmpCount++;
        }

        return tmpCount;
    }

    public T WaitConsume() {
        throw new NotImplementedException();
    }

    public void WaitProduce(T obj) {
        throw new NotImplementedException();
    }
}
