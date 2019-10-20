using System;
using System.Collections.Generic;
using System.Linq;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Misc;

namespace DotnetSourceLink.Indexing
{
    internal sealed class TypeNode : AbstractNode
    {
        private const bool UsePrimitiveTypeOffsetLocator = true;
        private readonly ISet<MemberLocation> _locations = new HashSet<MemberLocation>();

        public IReadOnlyCollection<MemberLocation> Locations => _locations.ToList().AsReadOnly();
        public bool IsPartial { get; }
        public string[] TypeParameters { get; }
        public byte TypeParameterCount => (byte)(TypeParameters?.Length ?? 0);

        public TypeNode(string fullQualifier, string[] typeParams, AbstractNode parent, bool isPartial, MemberLocation location)
            : base(fullQualifier, parent)
        {
            IsPartial = isPartial;
            _locations.Add(location);
            TypeParameters = typeParams;
        }

        public void AddType(TypeNode type)
        {
            if (!IsPartial || !type.IsPartial) { throw new ArgumentException("Cannot add location, must be partial class"); }

            _locations.UnionWith(type._locations);
            foreach (var member in type.Members.SelectMany(x => x.Value))
            {
                AddMember(member);
            }
        }

        public IGenericTypeOffsetLocator GetTypeParameterHierachy(IEnumerable<string> currentDepthParameters = null)
        {
            // Use PrimitiveTypeOffsetLocator for now, because of nested type ambiguity
            if (UsePrimitiveTypeOffsetLocator)
            {
                return new PrimitiveTypeOffsetLocator(TypeParameters, currentDepthParameters?.ToArray());
            }

            TypeNode parent = this;

            var typeParamStack = new GenericTypeParameterHierachy();

            if (currentDepthParameters != null)
            {
                typeParamStack.AddAtCurrentDepth(currentDepthParameters);
                typeParamStack.AddAtNewDepth(TypeParameters);
            }

            else if (TypeParameters != null)
            {
                typeParamStack.AddAtCurrentDepth(TypeParameters);
            }

            while (parent.ParentNode is TypeNode parentNode)
            {
                typeParamStack.AddAtNewDepth(parent.TypeParameters);
                parent = parentNode;
            }

            return typeParamStack;
        }
    }
}
