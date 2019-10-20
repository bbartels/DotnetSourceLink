using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Member
{
    internal abstract class AbstractMember : ISourceElement
    {
        public NameStructure Identifier { get; }
        public AbstractNode Parent { get; }
        public MemberLocation Location { get; }

        protected AbstractMember(NameStructure identifier, AbstractNode parent, MemberLocation location)
        {
            Identifier = identifier;
            Parent = parent;
            Location = location;
        }
    }
}
