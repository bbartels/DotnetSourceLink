using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Indexing;
using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Misc
{
    internal static class ClassDeclarationSyntaxExtensions
    {
        private const char NestedClassDelimiter = '.';
        private const char NamespaceClassDelimiter = '.';
        private const char TypeArgDelimiter = '`';

        public static string GetFullName(this CSharpSyntaxNode source)
        {
            var items = new Stack<(string identifier, byte typeArgCount)>();
            var parent = source.Parent;

            while (parent is TypeDeclarationSyntax type)
            {
                items.Push((type.Identifier.Text, (byte)(type.GetTypeParameters()?.Length ?? 0)));
                parent = parent.Parent;
            }

            var @namespace = parent as NamespaceDeclarationSyntax
                ?? throw new ArgumentNullException();


            var sb = new StringBuilder().Append(@namespace.Name).Append(NamespaceClassDelimiter);

            foreach (var (identifier, typeArgCount) in items)
            {
                sb.Append(identifier);
                if (typeArgCount > 0) { sb.Append(TypeArgDelimiter).Append(typeArgCount); }
                sb.Append(NestedClassDelimiter);
            }
            sb.Remove(sb.Length - 1, 1);
            //sb.Append(source.Identifier.Text);

            /*
            if (source.TypeParameterList is { } typeParamList)
            {
                sb.Append(TypeParameterStart);
                foreach (var typeParam in typeParamList.Parameters)
                {
                    sb.Append(typeParam.Identifier.Text).Append(TypeParameterDelimiter);
                }

                sb.Remove(sb.Length - 1, 1).Append(TypeParameterEnd);
            }*/

            return sb.ToString();
        }

        public static string[] GetTypeParameters(this TypeDeclarationSyntax syntax)
            => syntax?.TypeParameterList?.Parameters.Select(x => x.Identifier.Text).ToArray() ?? null;

        public static bool IsPartial(this ClassDeclarationSyntax source) => source.Modifiers.Any(x => x.Text == "partial");
        public static bool IsPublic(this ClassDeclarationSyntax source) => source.Modifiers.Any(x => x.Text == "public");

        public static string[] GetTypeParameters(this StructDeclarationSyntax syntax)
            => syntax?.TypeParameterList?.Parameters.Select(x => x.Identifier.Text).ToArray() ?? null;
        public static bool IsPartial(this StructDeclarationSyntax source) => source.Modifiers.Any(x => x.Text == "partial");
        public static bool IsPublic(this StructDeclarationSyntax source) => source.Modifiers.Any(x => x.Text == "public");
    }

    internal static class MethodDeclarationSyntaxExtensions
    {
        private static readonly Dictionary<string, char?> ModifierDictionary = new Dictionary<string, char?>
        {
            { "ref",    '@' },
            { "in",     null },
            { "out",    null },
            { "params", null },
        };

        private static bool HasModifier(SyntaxTokenList list)
            => list.Any() && ModifierDictionary.ContainsKey(list.First().Text);

        public static bool IsPublic(this MethodDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public");

        public static Parameter[] GetParameters(this ConstructorDeclarationSyntax syntax, TypeNode parent)
        {
            var hierachy = parent.GetTypeParameterHierachy();
            var typeConverter = new TypeSyntaxConverter(hierachy);
            return syntax?.ParameterList.Parameters.Select(x => ToParameter(x, typeConverter)).ToArray();
        }

        public static Parameter[] GetParameters(this MethodDeclarationSyntax syntax, TypeNode parent, string[] methodTypeParams)
        {
            var hierachy = parent.GetTypeParameterHierachy(methodTypeParams);
            var typeConverter = new TypeSyntaxConverter(hierachy);

            return syntax?.ParameterList.Parameters.Select(x => ToParameter(x, typeConverter)).ToArray();
        }

        private static Parameter ToParameter(ParameterSyntax param, TypeSyntaxConverter typeConverter)
        {
            var paramType = typeConverter.ConvertTypeStructure(param.Type);
            return new Parameter(paramType, HasModifier(param.Modifiers));
        }

        public static IEnumerable<string> GetTypeParameters(this MethodDeclarationSyntax syntax)
            => (syntax?.TypeParameterList?.Parameters.Select(x => x.Identifier.Text) ?? Enumerable.Empty<string>());
    }

    internal static class MemberDeclarationSyntaxExtensions
    {
        public static (ushort, ushort) GetLineNumber(this MemberDeclarationSyntax syntax)
            => ((ushort)(syntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1), (ushort)(syntax.GetLocation().GetLineSpan().EndLinePosition.Line + 1));

        public static bool IsPublic(this EventFieldDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public");

        public static bool IsPublic(this EnumDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public");

        public static bool IsPublic(this InterfaceDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public");

        public static bool IsPublic(this PropertyDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public") || syntax.ExplicitInterfaceSpecifier != null;

        public static bool IsPublic(this FieldDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public");

        public static bool IsPublic(this ConstructorDeclarationSyntax syntax)
            => syntax.Modifiers.Any(x => x.Text == "public");

        public static bool IsExplicit(this PropertyDeclarationSyntax syntax)
            => syntax.Modifiers.Count == 0;
    }
}
