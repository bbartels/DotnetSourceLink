using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Parser
{
    internal class InternalPropertySyntax : ISyntax
    {
        public TypeIdentifier Type { get; }
        public NameStructure Identifier { get; }

        public InternalPropertySyntax((TypeIdentifier type, NameStructure identifier) property)
        {
            Type = property.type;
            Identifier = property.identifier;
        }

        public string FullQualifiedIdentifier => Type.ToString() + '.' + Identifier.ToString().Replace('.', '#');

        public override string ToString() => "P:" + FullQualifiedIdentifier;
    }
}
