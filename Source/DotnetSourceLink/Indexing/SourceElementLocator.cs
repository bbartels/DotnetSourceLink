using System;
using System.Linq;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser;
using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing
{
    internal static class SourceElementLocator
    {
        public static ISourceElement LocateElement(SourceElementRequest request, AbstractNode node)
        {
            return request.Syntax switch
            {
                InternalMethodSyntax method => LocateMethodMember(method, node),
                _ => throw new ArgumentException() 
            };
        }

        private static MethodMember LocateMethodMember(InternalMethodSyntax syntax, AbstractNode node)
        {
            if (!node.Members.ContainsKey(syntax.Identifier.ToString())) { throw new ArgumentException("No method found"); }

            var elems = node.Members[syntax.Identifier.ToString()]
                                .OfType<MethodMember>()
                                .Where(MethodFilter);

            bool MethodFilter(MethodMember member)
            {
                return member.TypeArguments.Length == syntax.TypeArguments
                    && member.Parameters.Count() == syntax.Parameters.Count();
            }
            return elems.First();
        }

        private static TypeNode LocateTypeNode()
        {
            return null;
        }
    }
}
