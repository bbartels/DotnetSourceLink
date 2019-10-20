using System;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Indexing.Locator;
using DotnetSourceLink.Parser;

namespace DotnetSourceLink.Indexing
{
    internal sealed class SourceElementManager
    {
        private readonly AbstractNode _rootNode;

        public SourceElementManager(AbstractNode rootNode)
        {
            _rootNode = rootNode;
        }

        public void InsertElement(string path, ISourceElement element)
        {
            AbstractNode currentNode = FindNode(path);
            switch (element)
            {
                case AbstractMember member:
                {
                    currentNode.AddMember(member);
                } break;

                case AbstractNode node:
                {
                    currentNode.AddNode(node);
                } break;
            }
        }


        public ISourceElement FindElement(Parser.Model.SourceElementRequest request)
        {
            return request.Syntax switch
            {
                InternalTypeSyntax        type => new TypeNodeLocator(_rootNode, type.Type).Locate(),
                InternalMethodSyntax    method => new MethodMemberLocator(_rootNode, method).Locate(),
                InternalFieldSyntax      field => new FieldMemberLocator(_rootNode, field).Locate(),
                InternalPropertySyntax   field => new PropertyMemberLocator(_rootNode, field).Locate(),
                InternalEventSyntax eventField => new EventMemberLocator(_rootNode, eventField).Locate(),
                _ => throw new ArgumentException()
            };
        }

        private AbstractNode FindNode(string path)
        {
            AbstractNode currentNode = _rootNode;
            foreach(var (identifier, typeArgCount) in new Misc.IdentifierEnumerator(path))
            {
                if (!currentNode.ContainsNode(identifier))
                {
                    currentNode.AddNode(new NamespaceNode(identifier, currentNode));
                }

                currentNode = currentNode.GetSingleNode(identifier, typeArgCount);
            }
            return currentNode;
        }


        class NodeNotFoundException : Exception { }
    }
}
