using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace refslowness
{
  public class 	CircularBuffer<T> : IEnumerable<T>
  {
    readonly T[] _buffer;
    int _head;

    public CircularBuffer(int capacity)
    {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException(nameof(capacity), "must be positive");
      _buffer = new T[capacity];
      _head = 0;
      Capacity = capacity;
      Enqueue = InitialEnqueue;
    }

    public int Count { get; private set; }

    public int Capacity { get; }

    public delegate T AddDelegate(T v);

    public T Last { get; private set; }

    /// <summary>
    /// Adds the specified input value from the data structure.
    /// </summary>
    /// <param name="d">The value to add</param>
    public AddDelegate Enqueue { get; private set; }

    public unsafe T InitialEnqueue(T d)
    {
      Count++;
      _buffer[_head] = Last = d;

      ItemAdded?.Invoke(this, d);

      if (_head < Capacity)
        return d;

      // The sliding window is full, change behaviour from now on
      Enqueue = CircularEnqueue;
      _head = 0;
      return d;
    }

    public delegate void NewValue(CircularBuffer<T> buf, T value);

    public event NewValue ItemAdded;

    public unsafe T CircularEnqueue(T d)
    {
      _buffer[_head] = Last = d;

      if (_head == Capacity)
        _head = 0;

      ItemAdded?.Invoke(this, d);

      return d;
    }

    public void Clear()
    {
      _head = 0;
      Count = 0;
    }

    public T this[int index]
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get
      {
#if DEBUG
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException(nameof(index), $"Bad index {index} was supplied, Count is {Count}");
        if (Count != Capacity)
          throw new InvalidOperationException("The indexer may not be called before the buffer is full");
#endif

        return _buffer[(_head + index) % Capacity];
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      if (Count == 0 || Capacity == 0)
        yield break;

      for (var i = 0; i < Count; ++i)
        yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
