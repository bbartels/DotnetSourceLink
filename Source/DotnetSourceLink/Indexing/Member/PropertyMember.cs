using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Member
{
    internal sealed class PropertyMember : AbstractMember
    {
        public PropertyMember(NameStructure identifier, MemberLocation location)
            : base(identifier, null, location) { }
    }
}
