using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Parser
{
    internal class InternalMethodSyntax : ISyntax
    {
        public TypeIdentifier Type { get; }
        public NameStructure Identifier { get; }
        public byte TypeArguments { get; }
        public Parameter[] Parameters { get; }
        public byte ParameterCount => (byte) (Parameters?.Length ?? 0);

        public string FullQualifiedIdentifier => Type.Identifier + '.' + Identifier;

        public InternalMethodSyntax((TypeIdentifier, NameStructure, byte?) signature, IEnumerable<Parameter> parameters)
        {
            var (type, identifier, typeArgs) = signature;
            Type = type;
            Identifier = identifier;
            TypeArguments = typeArgs ?? 0;
            Parameters = parameters?.ToArray();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool anyParams = Parameters?.Any() ?? false;
            if (Parameters?.Any() ?? false)
            {
                foreach (var arg in Parameters)
                {
                    sb.Append(arg);

                    sb.Append(',');
                }
                sb.Remove(sb.Length - 1, 1);
            }

            return $"M:{Type}.{Identifier.ToString().Replace('.', '#')}{(TypeArguments > 0 ? $"``{TypeArguments}" : "")}{(anyParams ? $"({sb})" : "")}";
        }
    }
}
