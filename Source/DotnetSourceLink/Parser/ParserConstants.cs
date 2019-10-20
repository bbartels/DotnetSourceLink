using Sprache;

namespace DotnetSourceLink.Parser
{
    internal static class ParserConstants
    {
        public static readonly Parser<char> Comma =
            from comma in Parse.Char(',')
            select comma;

        public static readonly Parser<char> UnderScore =
            from underscore in Parse.Char('_')
            select underscore;

        public static readonly Parser<char> DotSeparator =
            from separator in Parse.Char('.')
            select separator;

        public static readonly Parser<char> ArgumentModifier =
            from modifier in Parse.Char('@')
            select modifier ;

        public static readonly Parser<char> PoundSeparator =
            from separator in Parse.Char('#')
            select separator;

        public static readonly Parser<char> ArrayOpen =
            from arrayOpen in Parse.Char('[')
            select arrayOpen;

        public static readonly Parser<char> ArrayClose =
            from arrayClose in Parse.Char(']')
            select arrayClose;

        public static readonly Parser<char> TupleOpen =
            from tupleOpen in Parse.Char('(')
            select tupleOpen;

        public static readonly Parser<char> TupleClose =
            from tupleClose in Parse.Char(')')
            select tupleClose;

        public static readonly Parser<char> MethodParamOpen =
            from open in Parse.Char('(')
            select open;

        public static readonly Parser<char> MethodParamClose =
            from close in Parse.Char(')')
            select close;

        public static readonly Parser<char> GenericOpen =
            from genericOpen in Parse.Char('{')
            select genericOpen;

        public static readonly Parser<char> GenericClose =
            from genericClose in Parse.Char('}')
            select genericClose;

        public static readonly Parser<char> Nullable =
            from nullable in Parse.Char('?')
            select nullable;

        public static readonly Parser<char> Pointer =
            from pointer in Parse.Char('*')
            select pointer;

        public static readonly Parser<char> TypeArgPrefix =
            from typeArgPref in Parse.Char('`')
            select typeArgPref;
    }
}
