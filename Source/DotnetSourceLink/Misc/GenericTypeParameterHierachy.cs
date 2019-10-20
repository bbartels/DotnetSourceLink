using System.Collections.Generic;
using System.Linq;

namespace DotnetSourceLink.Misc
{
    internal sealed class GenericTypeParameterHierachy : IGenericTypeOffsetLocator
    {
        private List<(string parameter, byte depth, byte index)> _typeParameters = new List<(string, byte, byte)>();
        private byte _currentIndex = 0;

        public byte CurrentDepth => _typeParameters.Count == 0 ? (byte)0 : _typeParameters[^1].depth;
        public byte NextDepth => _typeParameters.Count == 0 ? (byte)0 : (byte)(CurrentDepth + 1);

        public void AddAtCurrentDepth(string parameter) => _typeParameters.Add((parameter, CurrentDepth, _currentIndex++));
        public void AddAtCurrentDepth(IEnumerable<string> parameters)
        {
            foreach (var param in parameters)
            {
                AddAtCurrentDepth(param);
            }
        }

        public void AddAtNewDepth(string parameter) => _typeParameters.Add((parameter, NextDepth, (_currentIndex = 0)));

        public void AddAtNewDepth(string[] parameters)
        {
            _currentIndex = 0;
            var nextDepth = NextDepth;

            if (parameters != null)
            {
                _typeParameters.AddRange(parameters.Select(x => (x, nextDepth, _currentIndex++)));
            }
        }

        public (byte depth, byte index)? GetTypeParameterDepth(string parameter)
        {
            if (parameter is null) { return null; }

            for (int index = 0; index < _typeParameters.Count; index++)
            {
                var typeParam = _typeParameters[index];
                if (typeParam.parameter == parameter) { return (typeParam.depth, typeParam.index); }
            }

            return null;
        }
    }
}
