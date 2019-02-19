using System.Collections.Generic;

public delegate void Callback<T>(T obj);

internal interface ICircularBuffer<T> {
    int Capacity { get; }
    int Count { get; }
    bool IsEmpty { get; }
    bool IsFull { get; }

    void Produce(T obj);
    T Consume();
    void Clear();
    int ProduceAll(IEnumerable<T> add);
    void ConsumeAll(Callback<T> callback);
    void WaitProduce(T obj);
    T WaitConsume();
}