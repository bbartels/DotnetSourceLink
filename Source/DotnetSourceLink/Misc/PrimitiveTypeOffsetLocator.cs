using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSourceLink.Misc
{
    internal sealed class PrimitiveTypeOffsetLocator : IGenericTypeOffsetLocator
    {
        private string[] _typeLevelTypeArgs = null;
        private string[] _methodLevelTypeArgs = null;

        public PrimitiveTypeOffsetLocator(string[] typeTypeArgs, string[] methodTypeArgs = null)
        {
            _typeLevelTypeArgs = typeTypeArgs;
            _methodLevelTypeArgs = methodTypeArgs;
        }

        public (byte depth, byte index)? GetTypeParameterDepth(string identifier)
        {
            if (ListContainsIdentifier(_methodLevelTypeArgs, identifier, 0) is (byte _, byte _) methodResult) { return methodResult; }
            if (ListContainsIdentifier(_typeLevelTypeArgs, identifier, 1) is (byte _, byte _) typeResult) { return typeResult; }
            return null;
        }

        private static (byte depth, byte index)? ListContainsIdentifier(string[] typeArgs, string identifier, byte depth)
        {
            if (typeArgs == null) { return null; }
            return Array.FindIndex(typeArgs, x => x == identifier) switch
            { 
                -1 => ((byte, byte)?) null,
                int index => (depth, (byte)index)
            };
        }
    }
}
