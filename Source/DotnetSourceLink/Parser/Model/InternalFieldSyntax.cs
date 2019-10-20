using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Parser
{
    internal class InternalFieldSyntax : ISyntax
    {
        public TypeIdentifier Type { get; }
        public NameStructure Identifier { get; }

        public InternalFieldSyntax((TypeIdentifier type, NameStructure identifier) property)
        {
            Type = property.type;
            Identifier = property.identifier;
        }

        public string FullQualifiedIdentifier => Type.FullQualifiedIdentifier + '.' + Identifier;

        public override string ToString() => "F:" + FullQualifiedIdentifier;
    }
}
