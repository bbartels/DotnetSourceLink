using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Misc;
using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Locator
{
    internal sealed class TypeNodeLocator
    {
        private readonly AbstractNode _root;
        private readonly TypeIdentifier _type;

        public TypeNodeLocator(AbstractNode root, TypeIdentifier type)
        {
            _root = root;
            _type = type;
        }

        public ISourceElement Locate()
        {
            AbstractNode current = _root;
            foreach (var (identifier, typeArgCount) in new IdentifierEnumerator(_type.Namespace))
            {
                current = current.GetSingleNode(identifier, typeArgCount);
            }

            return current?.GetTypeNode(_type.Identifier, _type.TypeArgCount);
        }
    }
}
