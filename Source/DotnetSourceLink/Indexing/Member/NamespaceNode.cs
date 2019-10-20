using System.Collections.Generic;
using System.Linq;

namespace DotnetSourceLink.Indexing.Member
{
    internal sealed class NamespaceNode : AbstractNode
    {
        public NamespaceNode(string identifier, AbstractNode parent)
            : base(identifier, parent, Enumerable.Empty<AbstractNode>(), Enumerable.Empty<AbstractMember>()) { }
        public NamespaceNode(string identifier, AbstractNode parent,
            IEnumerable<AbstractNode> nodes, IEnumerable<AbstractMember> members)
            : base(identifier, parent, nodes, members) { }
    }
}
