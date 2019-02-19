using System;
using System.Collections.Generic;
using System.Text;

class CircularBuffer<T> : ICircularBuffer<T> {
    private T[] buffer;
    private BufferIndex writePointer;
    private BufferIndex readPointer;

    public int Capacity     { get; private set; }
    public int Count        { get; private set; }
    public bool IsEmpty =>  (0 == Count);
    public bool IsFull =>   (Count == Capacity);

    public CircularBuffer(int capacity) {
        buffer = new T[capacity];
        Capacity = capacity;
        writePointer = new BufferIndex(capacity);
        readPointer = new BufferIndex(capacity);
    }

    public void Clear() {
        Count = 0;
        writePointer.Index = 0;
        readPointer.Index = 0;
    }

    public T Consume() {
        if (0 == Count) {
            throw new BufferUnderflowException();
        }

        Count--;
        return buffer[readPointer.Index++];
    }

    public void ConsumeAll(Callback<T> callback) {
        throw new NotImplementedException();
    }

    public void Produce(T obj) {
        if (Count == Capacity) {
            throw new BufferOverflowException();
        }

        Count++;
        buffer[writePointer.Index++] = obj;
    }

    public int ProduceAll(IEnumerable<T> add) {
        throw new NotImplementedException();
    }

    public T WaitConsume() {
        throw new NotImplementedException();
    }

    public void WaitProduce(T obj) {
        throw new NotImplementedException();
    }
}
