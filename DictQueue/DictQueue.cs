using System.Collections;
using System.Diagnostics;

namespace DictQueue
{
    public interface IDicQueue<T>
    {
        public int Capacity { get; set; }
        public long TimeLimitSecond { get; set; }
        public long TimeLimitMillisecond { get; set; }
        
        public int Count { get; }
        public T this[long key] { get; }
        public IReadOnlyDictionary<long, T> Dict { get; }

        public IEnumerable<long> Keys { get; }
        public IEnumerable<T> Values { get; }
        
        public bool ContainsKey(long key);
        public bool TryGetValue(long key, out T value);
        public long EnQueue(T data);
        public T DeQueue();
        public T Peek();
        public bool Remove(long id);
        public void Clear();
        public void Shrink();
        public bool PeekOvertime();
        public bool DeQueueOvertime();

    }
    
    public interface IReadOnlyDicQueue<T>
    {
        public int Capacity { get; set; }
        public long TimeLimitSecond { get; set; }
        public long TimeLimitMillisecond { get; set; }
        
        public int Count { get; }
        public T this[long key] { get; }
        public IReadOnlyDictionary<long, T> Dict { get; }

        public IEnumerable<long> Keys { get; }
        public IEnumerable<T> Values { get; }
        
        public bool ContainsKey(long key);
        public bool TryGetValue(long key, out T value);
        public T Peek();
        public bool PeekOvertime();
    }

    public class DictQueue<T> : DictQueue,
        IDicQueue<T>,
        IReadOnlyDicQueue<T>,
        IReadOnlyCollection<T>,
        IReadOnlyDictionary<long, T>
    {
        public int Count => _queue.Count;

        public T this[long key]
        {
            get
            {
                if (!_dict.TryGetValue(key, out var node))
                    throw new ArgumentOutOfRangeException(nameof(key));
                return node.Value.Data;
            }
        }

        public IEnumerable<long> Keys => _dict.Keys;
        public IEnumerable<T> Values => this;
        public IReadOnlyDictionary<long, T> Dict => this;
        
        public int Capacity
        {
            get => _capacity;
            set
            {
                _capacity = value;
                if (_capacity < 0) return;
                while (_handlePool.Count > _capacity)
                    _handlePool.Pop();
            }
        }

        public long TimeLimitSecond
        {
            get => _timeLimit / 1000;
            set => _timeLimit = value * 1000L;
        }

        public long TimeLimitMillisecond
        {
            get => _timeLimit;
            set => _timeLimit = value;
        }
        
        public DictQueue() {}
        public DictQueue(int capacity) =>  _capacity = capacity;
        public DictQueue(int capacity, long timeLimit)
        {
            _capacity = capacity;
            _timeLimit = timeLimit;
        }
        
        
        public bool ContainsKey(long key)
        {
            return _dict.ContainsKey(key);
        }

        public bool TryGetValue(long key, out T value)
        {
            if (_dict.TryGetValue(key, out var node))
            {
                value = node.Value.Data;
                return true;
            }

            value = default!;
            return false;
        }

        public long EnQueue(T data)
        {
            var node = DePool();
            node.Value.Data = data;
            node.Value.Id = IdGen(node.GetHashCode());
            node.Value.Tick = Clock.ElapsedMilliseconds;
            
            _dict.Add(node.Value.Id, node);
            _queue.AddLast(node);
            return node.Value.Id;
        }

        public T DeQueue()
        {
            if (_queue.Count <= 0) return default!;
            
            var node = _queue.First;
            var data = node!.Value.Data;
            
            _queue.RemoveFirst();
            _dict.Remove(node.Value.Id);
            EnPool(node);
            
            return data;
        }

        public T Peek()
        {
            return _queue.Count <= 0 ? default! : _queue.First!.Value.Data;
        }

        public bool Remove(long id)
        {
            if (!_dict.TryGetValue(id, out var node)) return false;
            _dict.Remove(id);
            _queue.Remove(node);
            EnPool(node);
            return true;
        }

        public void Clear()
        {
            _dict.Clear();
            _queue.Clear();
        }

        public void Shrink()
        {
            _handlePool.Clear();
        }

        public bool PeekOvertime()
        {
            if (_queue.Count <= 0 || _timeLimit < 0) return false;
            return _timeLimit <= 
                   Clock.ElapsedMilliseconds - _queue.First!.Value.Tick;
        }

        public bool DeQueueOvertime()
        {
            if (!PeekOvertime()) return false;
            while (PeekOvertime()) DeQueue();
            return true;
        }

        IEnumerator<KeyValuePair<long, T>> IEnumerable<KeyValuePair<long, T>>.GetEnumerator()
        {
            // ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var (k, v) in _dict)
                yield return new KeyValuePair<long, T>(k, v.Value.Data);
            // ReSharper restore ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        }

        public IEnumerator<T> GetEnumerator()
        {
            // ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var v in _queue) yield return v.Data;
            // ReSharper restore ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Inner
        
        private void EnPool(LinkedListNode<DataHandle> node)
        {
            if (_handlePool.Count >= _capacity) return;
            node.Value.Data = default!;
            _handlePool.Push(node);
        }

        private LinkedListNode<DataHandle> DePool()
        {
            return _handlePool.Count <= 0 
                ? new LinkedListNode<DataHandle>(new DataHandle()) 
                : _handlePool.Pop();
        }

        private int _capacity = -1;
        private long _timeLimit = -1;
        
        private readonly LinkedList<DataHandle> _queue = new();
        private readonly Stack<LinkedListNode<DataHandle>> _handlePool = new();
        private readonly Dictionary<long, LinkedListNode<DataHandle>> _dict = new();

        private class DataHandle
        {
            public long Id;
            public long Tick;
            public T Data = default!;
        }
        
        #endregion
    }

    public class DictQueue
    {
        protected DictQueue() {}
        protected static readonly Stopwatch Clock = Stopwatch.StartNew();
        protected static long IdGen(long hashCode) => (Clock.ElapsedTicks << 32) | hashCode;
    }
}