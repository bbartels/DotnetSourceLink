using System.Collections.Generic;
using System.Linq;
using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Member
{
    internal sealed class MethodMember : AbstractMember
    {
        public string[] TypeArguments { get; }
        public Parameter[] Parameters { get; }

        public MethodMember(NameStructure identifier, AbstractNode parent, MemberLocation location,
            string[] typeArgs, IEnumerable<Parameter> parameters) : base(identifier, parent, location)
        {
            TypeArguments = typeArgs;
            Parameters = parameters.ToArray();
        }
    }
}
