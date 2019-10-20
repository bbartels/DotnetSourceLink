namespace DotnetSourceLink.Parser.Model
{
    internal sealed class TypeIdentifier
    {
        public string Namespace { get; }
        public string Identifier { get; }
        public byte TypeArgCount { get; }

        public TypeIdentifier(string @namespace, string identifier, byte? typeArgCount)
        {
            Namespace = @namespace;
            Identifier = identifier;
            TypeArgCount = typeArgCount ?? 0;
        }

        public TypeIdentifier(string fullIdentifier, byte? typeArgCount)
        {
            var index = fullIdentifier.LastIndexOf('.');
            Identifier = fullIdentifier.Substring(index + 1);
            Namespace = index == -1 ? null : fullIdentifier.Substring(0, index);
            TypeArgCount = typeArgCount ?? 0;
        }

        public override string ToString()
            => $"{(Namespace != null ? $"{Namespace}." : "")}{Identifier}{(TypeArgCount != 0 ? $"`{TypeArgCount}" : "")}";

        public string FullQualifiedIdentifier => ToString();
    }
}
