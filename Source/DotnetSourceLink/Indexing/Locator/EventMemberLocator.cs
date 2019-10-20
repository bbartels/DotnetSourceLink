using System.Linq;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser;

namespace DotnetSourceLink.Indexing.Locator
{
    internal sealed class EventMemberLocator : AbstractMemberLocator
    {
        private InternalEventSyntax _syntax;

        public EventMemberLocator(AbstractNode root, InternalEventSyntax eventSyntax) : base (root)
            => _syntax = eventSyntax;

        public override ISourceElement Locate()
        {
            var typeNode = new TypeNodeLocator(_root, _syntax.Type).Locate() as TypeNode;

            if (typeNode == null) { throw new SourceLinkLocateException($"Could not Locate TypeNode: { _syntax.Type }"); }

            return typeNode.GetMembers(_syntax.Identifier.Identifier)
                .OfType<EventMember>().Single(x => TypeStructureComparer.CompareTypes(x.Identifier, _syntax.Identifier));
        }
    }
}
