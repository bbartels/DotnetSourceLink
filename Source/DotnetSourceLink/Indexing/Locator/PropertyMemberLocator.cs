using System.Linq;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser;

namespace DotnetSourceLink.Indexing.Locator
{
    internal sealed class PropertyMemberLocator : AbstractMemberLocator
    {
        private readonly InternalPropertySyntax _syntax;

        public PropertyMemberLocator(AbstractNode root, InternalPropertySyntax syntax) : base(root)
        {
            _syntax = syntax;
        }

        public override ISourceElement Locate()
        {
            var typeNode = new TypeNodeLocator(_root, _syntax.Type).Locate() as TypeNode;

            if (typeNode == null) { throw new SourceLinkLocateException($"Could not Locate TypeNode: { _syntax.Type }"); }

            // TODO: Fix this to work with explicitly implemented properties!
            return typeNode.GetMembers(_syntax.Identifier.Identifier)
                .OfType<PropertyMember>()
                .Single(x => TypeStructureComparer.CompareTypes(x.Identifier, _syntax.Identifier));
        }
    }
}
