using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using DotnetSourceLink.Parser.Model;
using DotnetSourceLink.Misc;

namespace DotnetSourceLink.Indexing.Member
{
    internal class TypeSyntaxConverter
    {
        private static readonly Dictionary<string, string> PredefinedTypeDictionary = new Dictionary<string, string>
        {
            { "bool",  "System.Boolean" }, { "byte",     "System.Byte" }, { "sbyte",     "System.SByte" },
            { "short",   "System.Int16" }, { "ushort", "System.UInt16" }, { "char",       "System.Char" },
            { "int",     "System.Int32" }, { "uint",   "System.UInt32" },
            { "long",    "System.Int64" }, { "ulong",  "System.UInt64" },
            { "float",  "System.Single" }, { "double", "System.Double" }, { "decimal", "System.Decimal" },
            { "object", "System.Object" }, { "void",     "System.Void" }, { "string",   "System.String" },
        };

        private readonly IGenericTypeOffsetLocator _typeLocator;

        public TypeSyntaxConverter(IGenericTypeOffsetLocator typeLocator)
        {
            _typeLocator = typeLocator;
        }

        public TypeStructure ConvertTypeStructure(TypeSyntax syntax)
        {
            return syntax switch
            {
                PredefinedTypeSyntax ptSyntax => ToPredefinedIdentifierStructure(ptSyntax.Keyword.Text),
                IdentifierNameSyntax inSyntax => ToIdentifierStructure(inSyntax, _typeLocator),
                QualifiedNameSyntax qnSyntax => ToIdentifierStructure(qnSyntax),
                GenericNameSyntax gnSyntax => ToGenericTypeStructure(gnSyntax),
                ArrayTypeSyntax atSyntax => ToArrayTypeStructure(atSyntax),
                PointerTypeSyntax ptSyntax => ToPointerTypeStructure(ptSyntax),
                NullableTypeSyntax ntSyntax => ToNullableTypeStructure(ntSyntax),
                TupleTypeSyntax ttSyntax => ToTupleTypeStructure(ttSyntax),
                _ => throw new ArgumentException(syntax.GetType().ToString())
            };
        }

        private TupleTypeStructure ToTupleTypeStructure(TupleTypeSyntax syntax)
        {
            var tupleMembers = syntax.Elements.Select(x => ConvertTypeStructure(x.Type)).ToArray();
            return new TupleTypeStructure(tupleMembers);
        }

        private PointerTypeStructure ToPointerTypeStructure(PointerTypeSyntax syntax)
            => new PointerTypeStructure(ConvertTypeStructure(syntax.ElementType));

        private ArrayTypeStructure ToArrayTypeStructure(ArrayTypeSyntax syntax)
            => new ArrayTypeStructure(ConvertTypeStructure(syntax.ElementType));

        private GenericNameStructure ToNullableTypeStructure(NullableTypeSyntax syntax)
            => new GenericNameStructure("Nullable", new TypeStructure[] { ConvertTypeStructure(syntax.ElementType) });
            //=> new NullableTypeStructure(ConvertTypeStructure(syntax.ElementType));

        private TypeStructure ToPredefinedIdentifierStructure(string identifier)
        {
            var split = PredefinedTypeDictionary[identifier].Split('.');
            return new QualifiedNameStructure(new IdentifierStructure(split[0]), new IdentifierStructure(split[^1]));
        }

        private TypeStructure ToIdentifierStructure(IdentifierNameSyntax syntax, IGenericTypeOffsetLocator locator)
        {
            return locator.GetTypeParameterDepth(syntax.Identifier.Text) is (byte depth, byte index)
                ? (TypeStructure) new TypeArgStructure(depth, index)
                : new IdentifierStructure(syntax.Identifier.Text);
        }

        private QualifiedNameStructure ToIdentifierStructure(QualifiedNameSyntax syntax)
            => new QualifiedNameStructure(ConvertTypeStructure(syntax.Left), ConvertTypeStructure(syntax.Right) as SimpleNameStructure);

        private GenericNameStructure ToGenericTypeStructure(GenericNameSyntax syntax)
        {
            var arguments =
                syntax.TypeArgumentList.Arguments.Select(x => ConvertTypeStructure(x)).ToArray();

            return new GenericNameStructure(syntax.Identifier.Text, arguments);
        }
    }
}
