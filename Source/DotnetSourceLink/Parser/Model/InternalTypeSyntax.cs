using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Parser
{
    internal class InternalTypeSyntax : ISyntax
    {
        public TypeIdentifier Type { get; }

        public string FullQualifiedIdentifier => Type.FullQualifiedIdentifier;

        public InternalTypeSyntax(TypeIdentifier type) => Type = type;

        public override string ToString() => $"T:{Type}";
    }
}
