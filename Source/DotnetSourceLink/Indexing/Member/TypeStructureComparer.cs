using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using DotnetSourceLink.Parser.Model;

namespace DotnetSourceLink.Indexing.Member
{
    internal static class TypeStructureComparer
    {
        public static bool CompareTypes(TypeStructure first, TypeStructure second)
            => Compare(first, second);

        private static bool Compare(TypeStructure first, TypeStructure second)
        {
            return first switch
            {
                GenericNameStructure firstGNS when second is GenericNameStructure secondGNS
                    => CompareGenericTypeStructure(firstGNS, secondGNS),
                SimpleNameStructure firstSNS when second is SimpleNameStructure secondSNS
                    => CompareIdentifierStructure(firstSNS, secondSNS),
                QualifiedNameStructure firstQNS when second is QualifiedNameStructure secondQNS
                    => CompareQualifiedNameStructure(firstQNS, secondQNS),
                QualifiedNameStructure firstQNS when second is IdentifierStructure secondINS
                    => CompareQNSWithINS(firstQNS, secondINS),
                SimpleNameStructure firstINS when second is QualifiedNameStructure secondQNS
                    => CompareINSWithQns(firstINS, secondQNS),

                TypeArgStructure firstTAS when second is TypeArgStructure secondTAS
                    => CompareTypeArgStructure(firstTAS, secondTAS),
                TupleTypeStructure firstTTS when second is TupleTypeStructure secondTTS
                    => CompareTupleTypeStructure(firstTTS, secondTTS),
                PointerTypeStructure firstPTS when second is PointerTypeStructure secondPTS
                    => ComparePointerTypeStructure(firstPTS, secondPTS),
                ArrayTypeStructure firstATS when second is ArrayTypeStructure secondATS
                    => CompareArrayTypeStructure(firstATS, secondATS),
                NullableTypeStructure firstNTS when second is NullableTypeStructure secondNTS
                    => CompareNullableTypeStructure(firstNTS, secondNTS),

                _   => false
            };
        }

        private static bool CompareQNSWithINS(QualifiedNameStructure qns, IdentifierStructure ins)
            => throw new ArgumentException("This should reeaalllyyy not happen.");

        private static bool CompareINSWithQns(SimpleNameStructure ins, QualifiedNameStructure qns)
            => Compare(ins, qns.Right);

        private static bool CompareIdentifierStructure(SimpleNameStructure first, SimpleNameStructure second)
            => first.Identifier == second.Identifier;

        private static bool CompareTypeArgStructure(TypeArgStructure first, TypeArgStructure second)
            => first.Index == second.Index && first.Offset == second.Offset;

        private static bool CompareTupleTypeStructure(TupleTypeStructure first, TupleTypeStructure second)
        {
            return first.Structures.Length == second.Structures.Length &&
                   first.Structures.Zip(second.Structures, (First, Second) => (First, Second))
                                   .Select(x => Compare(x.First, x.Second))
                                   .All(x => x);
        }

        private static bool CompareGenericTypeStructure(GenericNameStructure first, GenericNameStructure second)
        {
            return  first.Identifier == second.Identifier &&
                    first.GenericTypeParameters.Length == second.GenericTypeParameters.Length &&
                    first.GenericTypeParameters.Zip(second.GenericTypeParameters, (First, Second) => (First, Second))
                                   .Select(x => Compare(x.First, x.Second))
                                   .All(x => x);
        }

        private static bool ComparePointerTypeStructure(PointerTypeStructure first, PointerTypeStructure second) => true;
        private static bool CompareArrayTypeStructure(ArrayTypeStructure first, ArrayTypeStructure second) => true;
        private static bool CompareNullableTypeStructure(NullableTypeStructure first, NullableTypeStructure second) => true;

        private static bool CompareQualifiedNameStructure(QualifiedNameStructure first, QualifiedNameStructure second)
        {
            if (!Compare(first.Right, second.Right)) { return false; }

            while (first.Left  is QualifiedNameStructure firstNext &&
                   second.Left is QualifiedNameStructure secondNext)
            {
                first = firstNext; second = secondNext;
                if (!Compare(first.Right, second.Right)) { return false; }
            }

            if (second.Left is QualifiedNameStructure secondLeft)
            {
                return Compare(first.Left, secondLeft.Right);
            }

            return Compare(first.Left, second.Left);
        }
    }
}
