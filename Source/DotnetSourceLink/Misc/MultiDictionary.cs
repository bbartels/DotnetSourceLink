using System;
using System.Collections.Generic;

namespace DotnetSourceLink.Misc
{
    internal interface IReadOnlyMultiDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, List<TValue>> { }

    internal sealed class MultiDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>, IReadOnlyMultiDictionary<TKey, TValue>
    {
        private readonly byte InitialCap;
        public MultiDictionary() { }

        public MultiDictionary(byte initialCap)
        {
            InitialCap = initialCap;
        }

        public void AddEntries(IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
        {
            foreach (var entry in values)
            {
                AddEntry(entry, keySelector);
            }
        }

        public void AddEntry(TValue value, Func<TValue, TKey> keySelector)
        {

            var key = keySelector(value);
            if (!ContainsKey(key)) { this[key] = new List<TValue>(InitialCap); }

            this[key].Add(value);
        }
    }
}
