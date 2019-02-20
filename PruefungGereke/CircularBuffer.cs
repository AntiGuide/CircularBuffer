using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class CircularBuffer<T> : ICircularBuffer<T> {
    private static AutoResetEvent isNotEmpty = new AutoResetEvent(false);
    private static AutoResetEvent isNotFull = new AutoResetEvent(false);

    private volatile T[] buffer;
    private BufferIndex writeIndex;
    private BufferIndex readIndex;
    private Mutex operating = new Mutex();
    private volatile int count;
    private readonly int capacity;

    public int Capacity { get => capacity; }
    public int Count { get => count; }
    public bool IsEmpty => (0 == count);
    public bool IsFull => (count == capacity);

    public CircularBuffer(int capacity) {
        buffer = new T[capacity];
        this.capacity = capacity;
        writeIndex = new BufferIndex(capacity);
        readIndex = new BufferIndex(capacity);
    }

    public void Clear() {
        count = 0;
        writeIndex.Index = 0;
        readIndex.Index = 0;
    }

    public T Consume() {
        if (IsEmpty) {
            throw new BufferUnderflowException();
        }

        T ret;
        Interlocked.Decrement(ref count);
        lock(readIndex){
            ret = buffer[readIndex.Index++];
        }

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
        if (IsFull) {
            throw new BufferOverflowException();
        }

        Interlocked.Increment(ref count);
        lock(writeIndex){
            buffer[writeIndex.Index++] = obj;
        }
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
        if (IsEmpty) {
            isNotEmpty.Reset();
            isNotEmpty.WaitOne();
        }

        operating.WaitOne();
        var ret = Consume();
        isNotFull.Set();
        operating.ReleaseMutex();
        return ret;
    }

    public void WaitProduce(T obj) {
        if (IsFull) {
            isNotFull.Reset();
            isNotFull.WaitOne();
        }

        operating.WaitOne();
        Produce(obj);
        isNotEmpty.Set();
        operating.ReleaseMutex();
    }
}
