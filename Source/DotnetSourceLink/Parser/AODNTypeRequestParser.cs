using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sprache;

using DotnetSourceLink.Parser.Model;
using static DotnetSourceLink.Parser.ParserConstants;

namespace DotnetSourceLink.Parser
{
    public class AODNTypeRequestParser
    {
        public string SourceRequest { get; }

        private static readonly Parser<string> Identifier =
            from first in Parse.Letter.Or(UnderScore).Or(PoundSeparator)
            from remaining in Parse.LetterOrDigit.Or(UnderScore).Many().Text()
            select first + remaining;

        private static readonly Parser<string> PropertyIdentifier =
            from first in Parse.Letter.Or(UnderScore).Or(GenericOpen).Or(GenericClose)
            from remaining in Parse.LetterOrDigit.Or(UnderScore).Or(GenericOpen).Or(GenericClose).Many().Text()
            select first + remaining;

        private static readonly Parser<(NameStructure, byte?)> PropertyIdentifiers =
            from identifier in PropertyIdentifier.DelimitedBy(PoundSeparator)
            select ((NameStructure, byte?)) (ToNameStructure(identifier.ToArray()), null);

        private static readonly Parser<string[]> ParamTypeIdentifier =
            from name in Identifier.DelimitedBy(DotSeparator).Select(x => x.ToArray())
            select name;

        private static readonly Parser<bool> Array =
            from open in ArrayOpen
            from close in ArrayClose
            from nullable in ParserConstants.Nullable.Optional()
            select !nullable.IsEmpty;

        private static readonly Parser<TypeStructure[]> GenericTypeArguments =
            from open in GenericOpen
            from type in TypeStructure.DelimitedBy(Comma).Select(x => x.ToArray())
            from close in GenericClose
            select type;

        private static readonly Parser<TypeStructure> TupleType =
            from open in TupleOpen
            from types in TypeStructure.DelimitedBy(Comma)
            from close in TupleClose
            from nullable in ParserConstants.Nullable.Optional()
            from arrays in Array.Many().Optional()
            select ToTypeStructure(types, nullable, arrays);

        private static readonly Parser<TypeStructure> TypeArg =
            from offset in TypeArgPrefix.Many().Select(x => (byte)(x.Count()))
            from index in Parse.Number.Select(byte.Parse)
            from nullable in ParserConstants.Nullable.Optional()
            from pointer in Pointer.Many().Optional()
            from arrays in Array.Many().Optional()
            select ToTypeStructure(new TypeArgStructure((byte)(2 - offset), index), nullable, pointer, arrays);

        private static readonly Parser<TypeStructure> ConcreteType =
            from identifier in ParamTypeIdentifier
            from generic in GenericTypeArguments.Optional()
            from nullable in ParserConstants.Nullable.Optional()
            from pointer in Pointer.Many().Optional()
            from array in Array.Many().Optional()
            select ToTypeStructure(identifier, generic, nullable, pointer, array);

        private static readonly Parser<TypeStructure> TypeStructure =
            from type in ConcreteType.Or(TupleType).Or(TypeArg)
            select type;

        private static readonly Parser<(TypeArgumentKind, byte)> MethodTypeArgumentIdentifier =
            from _ in TypeArgPrefix.Then(c => TypeArgPrefix)
            from count in Parse.Number.Text().Select(Convert.ToByte)
            select (TypeArgumentKind.Member, count);

        private static readonly Parser<(TypeArgumentKind, byte)> TypeTypeArgumentIdentifier =
            from _ in TypeArgPrefix
            from count in Parse.Number.Token().Select(Convert.ToByte)
            select (TypeArgumentKind.Type, count);

        private static readonly Parser<NameStructure> MemberIdentifier =
            from identifiers in Identifier.DelimitedBy(PoundSeparator)
            select ToNameStructure(identifiers.ToArray());

        internal static NameStructure ParseIdentifier(string identifier)
            => MemberIdentifier.Parse(identifier);

        private static readonly Parser<IdIdentifier> IdTypeIdentifier =
            from id in Parse.Char('M').Return(IdIdentifier.Method)
                   .Or(Parse.Char('P').Return(IdIdentifier.Property))
                   .Or(Parse.Char('T').Return(IdIdentifier.Type))
                   .Or(Parse.Char('F').Return(IdIdentifier.Field))
                   .Or(Parse.Char('E').Return(IdIdentifier.Event))
            from _ in Parse.Char(':')
            select id;

        private static readonly Parser<string> TypeIdentifier =
           from identifier in Parse.AnyChar.Until(MethodParamOpen).Text().Or(Parse.AnyChar.Many().End()).Text()
           select identifier;

        private static NameStructure ToNameStructure(string[] identifiers)
        {
            if (identifiers.Length == 1) { return new IdentifierStructure(identifiers[0]); }
            return ParamIdentifier(identifiers[..^1], new IdentifierStructure(identifiers[^1]));
        }

        private static readonly Parser<TypeIdentifier> Type =
            from identifier in Identifier
            from argCount in TypeTypeArgumentIdentifier.Optional()
            select new TypeIdentifier(identifier, (argCount.IsDefined ? (byte?)argCount.GetOrDefault().Item2 : null));

        private static readonly Parser<(NameStructure, byte?)> MethodMemberIdentifier =
            from identifier in MemberIdentifier
            from typeArgId in MethodTypeArgumentIdentifier.Optional().Select(x => x.IsDefined ? (byte?)x.GetOrDefault().Item2 : null)
            select (identifier, typeArgId); 

        private static readonly Parser<Parameter> Parameter =
            from argument in TypeStructure
            from hasModifier in ArgumentModifier.Optional().Select(x => !x.IsEmpty)
            select new Parameter(argument, hasModifier);

        private static readonly Parser<IEnumerable<Parameter>> Parameters =
            from parameters in Parameter.DelimitedBy(Comma.Token())
            from close in MethodParamClose
            select parameters;

        private static readonly Parser<InternalMethodSyntax> MethodSyntax =
            from _ in IdTypeIdentifier
            from identifier in TypeIdentifier
            from parameters in Parameters.Optional()
            select new InternalMethodSyntax(ParseMember(identifier), parameters.GetOrDefault());

        private static readonly Parser<InternalPropertySyntax> PropertySyntax =
            from _ in IdTypeIdentifier
            from identifier in TypeIdentifier
            select new InternalPropertySyntax(ParseProperty(identifier));

        private static readonly Parser<InternalFieldSyntax> FieldSyntax =
            from _ in IdTypeIdentifier
            from identifier in TypeIdentifier
            select new InternalFieldSyntax(ParseProperty(identifier));

        private static readonly Parser<InternalEventSyntax> EventSyntax =
            from _ in IdTypeIdentifier
            from identifier in TypeIdentifier
            select new InternalEventSyntax(ParseProperty(identifier));

        private static readonly Parser<InternalTypeSyntax> TypeSyntax =
            from _ in IdTypeIdentifier
            from type in TypeIdentifier
            select ParseType(type);

        private static TypeStructure ToTypeStructure(IEnumerable<TypeStructure> types, IOption<char> nullable, IOption<IEnumerable<bool>> arrays)
        {
            TypeStructure structure = new TupleTypeStructure(types.ToArray());

            ToTypeStructure(nullable, ref structure);
            ToTypeStructure(arrays, ref structure);
            return structure;
        }

        private static NameStructure ParamIdentifier(ReadOnlySpan<string> qualifiers, SimpleNameStructure element)
        {
            if (qualifiers.Length == 0) { return element; }
            if (qualifiers.Length == 1)
            {
                return new QualifiedNameStructure(new IdentifierStructure(qualifiers[0]), element);
            }

            return ParamIdentifier(qualifiers[2..], new QualifiedNameStructure(new IdentifierStructure(qualifiers[0]), new IdentifierStructure(qualifiers[1])), element);

            static NameStructure ParamIdentifier(ReadOnlySpan<string> qualifiers, QualifiedNameStructure before, SimpleNameStructure element)
            {
                return qualifiers.Length == 0
                    ? new QualifiedNameStructure(before, element)
                    : ParamIdentifier(qualifiers[1..], new QualifiedNameStructure(before, new IdentifierStructure(qualifiers[0])), element);
            }
        }

        private static TypeStructure ToTypeStructure(string[] fullIdentifier, IOption<TypeStructure[]> generic, IOption<char> nullable,
            IOption<IEnumerable<char>> pointer, IOption<IEnumerable<bool>> arrays)
        {
            SimpleNameStructure nameStruct;

            if (!generic.IsEmpty)
            {
                var genericTypes = generic.GetOrDefault();
                nameStruct = new GenericNameStructure(fullIdentifier[^1], genericTypes);
            }
            else { nameStruct = new IdentifierStructure(fullIdentifier[^1]); }

            TypeStructure structure = ParamIdentifier(fullIdentifier[..^1], nameStruct) ?? nameStruct;

            return ToTypeStructure(structure, nullable, pointer, arrays);
        }

        private static TypeStructure ToTypeStructure(TypeStructure structure, IOption<char> nullable,
            IOption<IEnumerable<char>> pointer, IOption<IEnumerable<bool>> arrays)
        {
            ToTypeStructure(nullable, ref structure);
            ToTypeStructure(pointer, ref structure);
            ToTypeStructure(arrays, ref structure);

            return structure;
        }

        private static void ToTypeStructure(IOption<char> nullable, ref TypeStructure structure)
            => structure = !nullable.IsEmpty ? new NullableTypeStructure(structure) : structure;

        private static void ToTypeStructure(IOption<IEnumerable<bool>> arrays, ref TypeStructure structure)
        {
            if (!arrays.IsEmpty)
            {
                foreach (var array in arrays.GetOrDefault())
                {
                    structure = array
                        ? new NullableTypeStructure(new ArrayTypeStructure(structure))
                        : (TypeStructure)new ArrayTypeStructure(structure);
                }
            }
        }

        private static void ToTypeStructure(IOption<IEnumerable<char>> pointers, ref TypeStructure structure)
        {
            if (pointers.IsEmpty) { return; }

            for (int i = 0; i < pointers.GetOrDefault().Count(); i++)
            {
                structure = new PointerTypeStructure(structure);
            }
        }

        private static (TypeIdentifier type, NameStructure identifier, byte? args) ParseMember(string sourceStr, bool property = false)
        {
            Memory<string> source = sourceStr.Split('.');
            var (id, args) = (property ? PropertyIdentifiers.Parse(source.Span[^1]) : MethodMemberIdentifier.Parse(source.Span[^1]));
            return (ParseType(source[0..^1]), id, args);
        }

        private static (TypeIdentifier type, NameStructure identifier) ParseProperty(string sourceStr)
        {
            var (type, identifier, _) = ParseMember(sourceStr, true);
            return (type, identifier);
        }

        private static InternalTypeSyntax ParseType(string sourceStr)
        {
            return new InternalTypeSyntax(ParseType(sourceStr.Split('.')));
        }

        private static TypeIdentifier ParseType(Memory<string> source)
        {
            var type = Type.Parse(source.Span[^1]);
            string temp = type.Identifier;

            if (source.Length >= 2)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var str in source.Span[0..^1])
                {
                    sb.Append(str).Append('.');
                }
                sb.Append(type.Identifier);
                temp = sb.ToString();
            }

            return new TypeIdentifier(temp, type.TypeArgCount);
        }

        public AODNTypeRequestParser(string sourceRequest)
        {
            SourceRequest = sourceRequest;
        }

        public SourceElementRequest ParseRequest()
        {
            ISyntax parsedRequest = SourceRequest[0] switch
            {
                'M' => MethodSyntax.Parse(SourceRequest),
                'T' => TypeSyntax.Parse(SourceRequest),
                'P' => PropertySyntax.Parse(SourceRequest),
                'E' => EventSyntax.Parse(SourceRequest),
                'F' => FieldSyntax.Parse(SourceRequest),
                 _  => throw new SourceLinkParseException("Invalid Id")
            };

            return new SourceElementRequest(parsedRequest);
        }
    }

    internal class SourceLinkParseException : Exception
    {
        public SourceLinkParseException(string message) : base(message) { } 
    }
}
