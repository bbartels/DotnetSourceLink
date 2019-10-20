using System;
using System.Linq;
using System.Text;

namespace DotnetSourceLink.Parser.Model
{
    internal struct Parameter
    {
        public TypeStructure Type { get; }
        public bool HasModifier { get; set; }

        public Parameter(TypeStructure type, bool hasModifier)
        {
            Type = type;
            HasModifier = hasModifier;
        }

        public override string ToString() => $"{Type.ToString()}{(HasModifier ? "@" : "")}";
    }

    internal abstract class TypeStructure { }
    internal abstract class NameStructure : TypeStructure
    { 
        public abstract string Identifier { get; }

        public NameStructure AppendSimpleName(SimpleNameStructure structure)
            => new QualifiedNameStructure(this, structure);
    }
    internal abstract class SimpleNameStructure : NameStructure { }

    internal sealed class IdentifierStructure : SimpleNameStructure
    {
        public override string Identifier { get; }

        public IdentifierStructure(string identifier) => Identifier = identifier;

        public override string ToString() => Identifier;
    }

    internal sealed class QualifiedNameStructure : NameStructure
    {
        public override string Identifier => Right.Identifier;
        public TypeStructure Left { get; }
        public SimpleNameStructure Right { get; private set; }

        public QualifiedNameStructure(TypeStructure left, SimpleNameStructure right)
        {
            Left = left;
            Right = right;
        }

        public void SetRight(SimpleNameStructure structure) => Right = structure;

        public override string ToString()
            => $"{Left}.{Right}";
    }

    internal sealed class TypeArgStructure : TypeStructure
    {
        public byte Index { get; }
        public byte Offset { get; }

        public TypeArgStructure(byte offset, byte index)
        {
            Index = index;
            Offset = offset;
        }

        public override string ToString()
            => (new string(Enumerable.Range(0, -Offset + 2).Select(x => '`').ToArray())) + Index;
    }

    internal sealed class TupleTypeStructure : TypeStructure
    {
        public TypeStructure[] Structures;

        public TupleTypeStructure(TypeStructure[] structures)
        {
            if (structures.Length == 0) { throw new ArgumentException("Length must be > 0"); }
            Structures = structures;
        }

        public override string ToString()
        {
            var sb = new StringBuilder().Append('(');

            foreach (var structure in Structures)
            {
                sb.Append(structure.ToString()).Append(',');
            }

            sb.Replace(',', ')', sb.Length - 1, 1);

            return sb.ToString();
        }
    }

    internal abstract class NodeTypeStructure : TypeStructure
    {
        public TypeStructure ElementType { get; protected set; }

        protected NodeTypeStructure(TypeStructure elementType) => ElementType = elementType;
    }

    internal sealed class GenericNameStructure : SimpleNameStructure
    {
        public override string Identifier { get; }
        public TypeStructure[] GenericTypeParameters { get; }
        public GenericNameStructure(string identifier, TypeStructure[] typeParams)
        {
            Identifier = identifier;
            GenericTypeParameters = typeParams;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Identifier).Append('{');

            foreach (var param in GenericTypeParameters)
            {
                sb.Append(param.ToString()).Append(',');
            }

            sb.Replace(',', '}', sb.Length - 1, 1);
            return sb.ToString();
        }
    }

    internal sealed class PointerTypeStructure : NodeTypeStructure
    {
        public PointerTypeStructure(TypeStructure elementType) : base(elementType) { }

        public override string ToString() => ElementType.ToString() + "*";
    }

    internal sealed class ArrayTypeStructure : NodeTypeStructure
    {
        public ArrayTypeStructure(TypeStructure elementType) : base(elementType) { }

        public override string ToString() => ElementType.ToString() + "[]";
    }


    internal sealed class NullableTypeStructure : NodeTypeStructure
    {
        public NullableTypeStructure(TypeStructure elementType) : base(elementType) { }

        public override string ToString() => ElementType.ToString() + "?";
    }
}
