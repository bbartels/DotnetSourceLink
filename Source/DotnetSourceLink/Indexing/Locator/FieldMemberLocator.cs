using System.Linq;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser;

namespace DotnetSourceLink.Indexing.Locator
{
    internal sealed class FieldMemberLocator : AbstractMemberLocator
    {
        private InternalFieldSyntax _syntax;

        public FieldMemberLocator(AbstractNode root, InternalFieldSyntax field) : base (root)
        {
            _syntax = field;
        }

        public override ISourceElement Locate()
        {
            var typeNode = new TypeNodeLocator(_root, _syntax.Type).Locate() as TypeNode;

            if (typeNode == null) { throw new SourceLinkLocateException($"Could not Locate TypeNode: { _syntax.Type }"); }

            return typeNode.GetMembers(_syntax.Identifier.Identifier)
                .OfType<FieldMember>().Single(x => TypeStructureComparer.CompareTypes(x.Identifier, _syntax.Identifier));
        }
    }
}
