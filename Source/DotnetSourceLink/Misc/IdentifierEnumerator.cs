using System;

namespace DotnetSourceLink.Misc
{
    internal ref struct IdentifierEnumerator
    {
        private SpanSplitEnumerator<char> _enumerator;
        private ReadOnlySpan<char> _sequence;

        public IdentifierEnumerator GetEnumerator() => this;

        public (string identifier, byte typeArgCount) Current
        {
            get
            {
                byte typeArgCount = 0;
                ReadOnlySpan<char> identifier = _sequence[_enumerator.Current];
                var typeArgIdx = identifier.IndexOf('`');

                if (typeArgIdx != -1)
                {
                    typeArgCount = Convert.ToByte(byte.Parse(identifier.Slice(typeArgIdx + 1)));
                    identifier = identifier.Slice(0, typeArgIdx);
                }

                return (identifier.ToString(), typeArgCount);
            }
        }

        public bool MoveNext() => _enumerator.MoveNext();

        public IdentifierEnumerator(ReadOnlySpan<char> fullIdentifier, bool reverse = false)
        {
            _sequence = fullIdentifier;
            _enumerator = new SpanSplitEnumerator<char>(fullIdentifier, '.', reverse);
        }

        internal ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
        {
            private readonly ReadOnlySpan<T> _sequence;
            private readonly T _separator;
            private int _offset;
            private int _index;
            private bool _reverse;

            public SpanSplitEnumerator<T> GetEnumerator() => this;

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator, bool reverse)
            {
                _sequence = span;
                _separator = separator;
                _index = reverse ? span.Length : 0;
                _offset = 0;
                _reverse = reverse;
            }

            public Range Current => _reverse
                ? new Range(_index + 1, _sequence.Length - _offset)
                : new Range(_offset, _offset + _index - 1);

            public bool MoveNext()
            {
                if (_reverse)
                {
                    if (_index == -1) { return false; }

                    _offset += (_sequence.Length - _offset) - _index;
                    _index = _sequence[..^_offset].LastIndexOf(_separator);
                }

                else
                {
                    if (_sequence.Length - _offset < _index) { return false; }

                    var slice = _sequence.Slice(_offset += _index);
                    var nextIdx = slice.IndexOf(_separator);
                    _index = (nextIdx != -1 ? nextIdx : slice.Length) + 1;
                }

                return true;
            }
        }
    }
}
