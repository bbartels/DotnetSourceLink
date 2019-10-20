using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Parser
{
    internal class InternalEventSyntax : ISyntax
    {
        public TypeIdentifier Type { get; }
        public NameStructure Identifier { get; }

        public InternalEventSyntax((TypeIdentifier type, NameStructure identifier) property)
        {
            Type = property.type;
            Identifier = property.identifier;
        }

        public string FullQualifiedIdentifier => Type.FullQualifiedIdentifier + '.' + Identifier.ToString().Replace('.', '#');

        public override string ToString() => "E:" + FullQualifiedIdentifier;
    }
}
