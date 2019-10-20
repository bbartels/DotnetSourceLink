using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;


using DotnetSourceLink.Misc;
using DotnetSourceLink.Indexing;
using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser.Model;
using DotnetSourceLink.Parser;

namespace DotnetSourceLink
{
    internal sealed class RepositoryParser
    {
        private readonly SourceElementManager _elementManager;
        private readonly Repository _repository;
        private static readonly TypeSyntaxConverter TypeConverter = new TypeSyntaxConverter(new PrimitiveTypeOffsetLocator(null));

        private static readonly ConcurrentQueue<(NamespaceDeclarationSyntax syntax, string file)> NamespaceQueue = new ConcurrentQueue<(NamespaceDeclarationSyntax syntax, string file)>();
        private static readonly Queue<(ClassDeclarationSyntax syntax, string file, TypeNode parent)> ClassQueue = new Queue<(ClassDeclarationSyntax syntax, string file, TypeNode parent)>();
        private static readonly Queue<(StructDeclarationSyntax syntax, string file, TypeNode parent)> StructQueue = new Queue<(StructDeclarationSyntax syntax, string file, TypeNode parent)>();

        public RepositoryParser(SourceElementManager elementManager, Repository repository)
        {
            _elementManager = elementManager;
            _repository = repository;
        }

        public void Scan()
        {
            while (NamespaceQueue.TryDequeue(out var result))
            {
                ParseNamespace(result);
            }

            while (ClassQueue.TryDequeue(out var result))
            {
                ParseClass(result);
            }

            while (StructQueue.TryDequeue(out var result))
            {
                ParseStruct(result);
            }
        }

        internal AbstractMember ParseNode(SyntaxNode node, string file = null, TypeNode parent = null)
        {
            if (node is CompilationUnitSyntax compileUnit)
            {
                foreach (var member in compileUnit.Members)
                {
                    _ = ParseNode(member, file);
                }

                return null;
            }

            switch (node)
            {
                case MethodDeclarationSyntax method when method.IsPublic(): { return ParseMethod(method, file, parent); }
                case PropertyDeclarationSyntax property when property.IsPublic(): { return ParseProperty(property, file, parent); }
                case FieldDeclarationSyntax field when field.IsPublic(): { return ParseField(field, file, parent); }
                case ConstructorDeclarationSyntax constructor when constructor.IsPublic(): { return ParseConstructor(constructor, file, parent); }
                case EventFieldDeclarationSyntax eventField when eventField.IsPublic(): { return ParseEvent(eventField, file, parent); }
                case EnumDeclarationSyntax @enum when @enum.IsPublic(): { ParseEnum(@enum, file, parent); } break;
                //TODO: Implement interface parsing
                //case InterfaceDeclarationSyntax @interface when @interface.IsPublic(): { }

                case NamespaceDeclarationSyntax @namespace:
                {
                    NamespaceQueue.Enqueue((@namespace, file));
                } break;
                case ClassDeclarationSyntax @class when @class.IsPublic():
                {
                    ClassQueue.Enqueue((@class, file, parent));
                } break;

                case StructDeclarationSyntax @struct when @struct.IsPublic():
                {
                    StructQueue.Enqueue((@struct, file, parent));
                } break;
            }

            return null;
        }

        private EventMember ParseEvent(EventFieldDeclarationSyntax eventField, string path, TypeNode parent)
        {
            var events = eventField.Declaration.Variables
                .Select(x => new EventMember(new IdentifierStructure(x.Identifier.Text), new MemberLocation(_repository, path, eventField.GetLineNumber())));
            return events.Single();
        }

        private NameStructure ParseIdentifier(string identifier, ExplicitInterfaceSpecifierSyntax explicitSyntax)
        {
            if (explicitSyntax == null) { return new IdentifierStructure(identifier); }
            var explicitName = (TypeConverter.ConvertTypeStructure(explicitSyntax.Name) as NameStructure);
            return explicitName.AppendSimpleName(new IdentifierStructure(identifier));
        }

        private MethodMember ParseConstructor(ConstructorDeclarationSyntax constructor, string path, TypeNode parent)
        {
            return new MethodMember(new IdentifierStructure("#ctor"), parent, new MemberLocation(_repository, path, constructor.GetLineNumber()),
                null, constructor.GetParameters(parent));
        }

        private FieldMember ParseField(FieldDeclarationSyntax field, string path, TypeNode parent)
        {
            var fields = field.Declaration.Variables
                .Select(x => new FieldMember(new IdentifierStructure(x.Identifier.Text), new MemberLocation(_repository, path, field.GetLineNumber())));
            return fields.Single();
        }

        private PropertyMember ParseProperty(PropertyDeclarationSyntax property, string path, TypeNode parent)
        {
            return new PropertyMember(ParseIdentifier(property.Identifier.Text, property.ExplicitInterfaceSpecifier), new MemberLocation(_repository, path, property.GetLineNumber()));
        }

        private MethodMember ParseMethod(MethodDeclarationSyntax method, string path, TypeNode parent)
        {
            var typeParams = method.GetTypeParameters().ToArray();

            return new MethodMember(ParseIdentifier(method.Identifier.Text, method.ExplicitInterfaceSpecifier), parent,
                new MemberLocation(_repository, path, method.GetLineNumber()), typeParams, method.GetParameters(parent, typeParams));
        }

        void ParseNamespace((NamespaceDeclarationSyntax syntax, string file) @namespace)
        {
            foreach (var member in @namespace.syntax.Members)
            {
                _ = ParseNode(member, @namespace.file);
            }
        }

        private void ParseEnum(EnumDeclarationSyntax syntax, string file, TypeNode parent)
        {
            var members = syntax.Members.Select(x => new FieldMember(new IdentifierStructure(x.Identifier.Text), new MemberLocation(_repository, file, x.GetLineNumber())));
            var fullName = syntax.GetFullName();
            var type = new TypeNode(fullName + '.' + syntax.Identifier.Text, null, parent, false, new MemberLocation(_repository, file, syntax.GetLineNumber()));
            foreach (var member in members)
            {
                type.AddMember(member);
            }
            _elementManager.InsertElement(fullName, type);
        }

        private void ParseStruct((StructDeclarationSyntax syntax, string file, TypeNode parent) type)
        {
            var (typeNode, name) = ParseStruct1(type);
            _elementManager.InsertElement(name, typeNode);
        }

        private (TypeNode, string) ParseStruct1((StructDeclarationSyntax syntax, string file, TypeNode parent) type)
        {
            var (syntax, file, parent) = type;
            var fullName = syntax.GetFullName();

            var parsedClass = new TypeNode(fullName + '.' + syntax.Identifier.Text, syntax.GetTypeParameters(), parent, syntax.IsPartial(), new MemberLocation(_repository, file, syntax.GetLineNumber()));
            var parsedMembers = syntax.Members.Where(x => x.GetType() != typeof(ClassDeclarationSyntax)).Select(x => ParseNode(x, file, parsedClass)).Where(x => x != null);
            var parsedNestedClasses = syntax.Members.OfType<ClassDeclarationSyntax>().Select(x => ParseClass1((x, file, parsedClass)));

            foreach (var member in parsedMembers)
            {
                parsedClass.AddMember(member);
            }

            foreach (var nestedClass in parsedNestedClasses)
            {
                parsedClass.AddTypeNode(nestedClass.Item1);
            }

            return (parsedClass, fullName);

        }

        private void ParseClass((ClassDeclarationSyntax syntax, string file, TypeNode parent) type)
        {
            var (typeNode, name) = ParseClass1(type);
            _elementManager.InsertElement(name, typeNode);
        }

        private (TypeNode, string) ParseClass1((ClassDeclarationSyntax syntax, string file, TypeNode parent) type)
        {
            var (syntax, file, parent) = type;
            var fullName = syntax.GetFullName();

            var parsedClass = new TypeNode(fullName + '.' + syntax.Identifier.Text, syntax.GetTypeParameters(), parent, syntax.IsPartial(), new MemberLocation(_repository, file, syntax.GetLineNumber()));
            var parsedMembers = syntax.Members.Where(x => x.GetType() != typeof(ClassDeclarationSyntax)).Select(x => ParseNode(x, file, parsedClass)).Where(x => x != null);
            var parsedNestedClasses = syntax.Members.OfType<ClassDeclarationSyntax>().Select(x => ParseClass1((x, file, parsedClass)));

            foreach (var member in parsedMembers)
            {
                parsedClass.AddMember(member);
            }

            foreach (var nestedClass in parsedNestedClasses)
            {
                parsedClass.AddTypeNode(nestedClass.Item1);
            }

            return (parsedClass, fullName);
        }
    }
}
