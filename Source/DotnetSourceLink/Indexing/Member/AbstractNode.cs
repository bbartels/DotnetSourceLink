using System.Collections.Generic;
using System.Linq;

using DotnetSourceLink.Misc;

namespace DotnetSourceLink.Indexing.Member
{
    internal abstract class AbstractNode : ISourceElement
    {
        private readonly MultiDictionary<string, AbstractNode> _nodes = new MultiDictionary<string, AbstractNode>();
        private readonly MultiDictionary<string, AbstractMember> _members = new MultiDictionary<string, AbstractMember>();

        public string FullQualifier { get; }
        public string Identifier => FullQualifier.Substring(FullQualifier.LastIndexOf('.') + 1);
        public AbstractNode ParentNode { get; }
        public IReadOnlyMultiDictionary<string, AbstractMember> Members => _members;

        protected AbstractNode(string fullQualifier, AbstractNode parent)
            : this(fullQualifier, parent, Enumerable.Empty<AbstractNode>(), Enumerable.Empty<AbstractMember>()) { }

        protected AbstractNode(string fullQualifier, AbstractNode parent, IEnumerable<AbstractNode> nodes, IEnumerable<AbstractMember> members)
        {
            FullQualifier = fullQualifier;
            ParentNode = parent;
            foreach (var entry in nodes)
            {
                _nodes.AddEntry(entry, c => c.Identifier);
            }

            _members.AddEntries(members, c => c.Identifier.Identifier);
        }

        public bool ContainsNode(string identifier) => _nodes.ContainsKey(identifier);

        public AbstractNode GetSingleNode(string identifier)
            => _nodes.ContainsKey(identifier) ? _nodes[identifier].Single() : null;

        public AbstractNode GetSingleNode(string identifier, byte typeArgCount)
        {
            var node = GetNodes(identifier).OfType<TypeNode>().SingleOrDefault(x => x.TypeParameterCount == typeArgCount);
            return node ?? GetSingleNode(identifier);
        }

        public TypeNode GetTypeNode(string identifier, byte typeArgCount)
        {
            return _nodes.ContainsKey(identifier)
                ? _nodes[identifier].OfType<TypeNode>().First(x => x.TypeParameterCount == typeArgCount)
                : null;
        }

        public IEnumerable<AbstractNode> GetNodes(string identifier)
            => _nodes.ContainsKey(identifier) ? _nodes[identifier] : null;

        public IEnumerable<AbstractMember> GetMembers(string identifier) =>
            _members.ContainsKey(identifier) ? _members[identifier] : null;

        public void AddMember(AbstractMember member)
            => _members.AddEntry(member, c => c.Identifier.Identifier);

        public void AddTypeNode(TypeNode member)
            => _nodes.AddEntry(member, c => c.Identifier);

        public void AddNode(AbstractNode node)
        {
            switch (node)
            {
                case TypeNode type when _nodes.ContainsKey(type.Identifier):
                {
                    try
                    {

                        var types = _nodes[type.Identifier].OfType<TypeNode>()
                            .Where(c => c.TypeParameterCount == type.TypeParameterCount && c.IsPartial).ToArray();
                        if (types.Length == 1 && type.IsPartial)
                        {
                            types[0].AddType(type);
                        }
                        else
                        {
                            _nodes.AddEntry(type, c => c.Identifier);
                        }
                    }
                    catch (System.Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                } break;
                case TypeNode type:
                case NamespaceNode @namespace:
                {
                    _nodes.AddEntry(node, c => c.Identifier);
                } break;
            }
        }
    }
}
