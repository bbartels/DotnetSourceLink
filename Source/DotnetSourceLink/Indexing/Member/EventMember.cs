using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Member
{
    internal sealed class EventMember : AbstractMember
    {
        public EventMember(NameStructure identifier, MemberLocation location)
            : base(identifier, null, location) { }
    }
}
