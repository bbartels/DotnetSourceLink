using System.Linq;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser;
using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Locator
{
    internal sealed class MethodMemberLocator : AbstractMemberLocator
    {
        private readonly InternalMethodSyntax _syntax;

        public MethodMemberLocator(AbstractNode root, InternalMethodSyntax method) : base(root)
        {
            _syntax = method;
        }

        public override ISourceElement Locate()
        {
            var typeNode = new TypeNodeLocator(_root, _syntax.Type).Locate() as TypeNode;

            if (typeNode == null) { throw new SourceLinkLocateException($"Could not Locate TypeNode: { _syntax.Type }"); }

            return typeNode
                    .GetMembers(_syntax.Identifier.Identifier)
                    .OfType<MethodMember>()
                    .Single(x => (x?.TypeArguments?.Length ?? 0) == _syntax.TypeArguments && MethodArgumentsAreEqual(x.Parameters));
        }

        private bool MethodArgumentsAreEqual(Parameter[] parameters)
        {
            if (_syntax.ParameterCount != (parameters?.Length ?? 0)) { return false; }

            return parameters.Zip(_syntax.Parameters, (First, Second) => (First, Second)).All(x => ParameterIsEqual(x.Second, x.First));
        }

        private bool ParameterIsEqual(Parameter eParam, Parameter iParam)
        {
            return iParam.HasModifier == eParam.HasModifier
                && TypeStructureComparer.CompareTypes(iParam.Type, eParam.Type);
        }
    }
}
