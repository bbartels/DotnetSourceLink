using System;
using System.Collections.Concurrent;

namespace DotnetSourceLink.Misc
{
    internal sealed class ConcurrentBictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _keyDictionary = new ConcurrentDictionary<TKey, TValue>();
        private readonly ConcurrentDictionary<TValue, TKey> _valueDictionary = new ConcurrentDictionary<TValue, TKey>();

        public void Add(TKey key, TValue value)
        {
            lock (_valueDictionary)
            {
                bool mismatchedInsert = _keyDictionary.TryAdd(key, value) != _valueDictionary.TryAdd(value, key);
                if (mismatchedInsert) { throw new ArgumentException("Mismatched insert in Bictionary."); }
            }
        }

        public TKey this[TValue val]
        {
            get => _valueDictionary[val];
            set => Add(value, val);
        }

        public TValue this[TKey key]
        {
            get => _keyDictionary[key];
            set => Add(key, value);
        }

        public bool ContainsKey(TKey key) => _keyDictionary.ContainsKey(key);
        public bool ContainsKey(TValue key) => _valueDictionary.ContainsKey(key);
    }
}
