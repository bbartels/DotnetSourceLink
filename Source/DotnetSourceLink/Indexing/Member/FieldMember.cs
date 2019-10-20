using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Member
{
    internal sealed class FieldMember : AbstractMember
    {
        public FieldMember(NameStructure identifier, MemberLocation location)
            : base(identifier, null, location) { }
    }
}
