using Xunit;

using DotnetSourceLink.Parser;

namespace DotnetSourceLink.Tests
{
    public class SourceLinkParserTests
    {
        [Theory]
        [InlineData("P:System.Collections.Generic.List`1.System#Collections#Generic#ICollection{T}#IsReadOnly")]
        [InlineData("M:System.UInt64.System#IConvertible#ToDecimal(System.IFormatProvider)")]
        [InlineData("M:System.Collections.Generic.List`1.CopyTo(System.Int32,`0[],System.Int32,System.Int32)")]
        [InlineData("M:System.WindowsRuntimeSystemExtensions.GetAwaiter``1(Windows.Foundation.IAsyncOperation{``0})")]
        [InlineData("M:System.Test(teststruct{teststruct{short?[],long?,double?},int?[],(string[]?,string?)?[]?[]?[]}?**[]?[]?)")]
        [InlineData("M:System.Convert.TryToBase64Chars(System.ReadOnlySpan{System.Byte[]},System.Span{System.Char},System.Int32@,System.Base64FormattingOptions)")]
        [InlineData("M:System.TupleExtensions.ToTuple``16(System.ValueTuple{``0,``1,``2,``3,``4,``5,``6,System.ValueTuple{``7,``8,``9,``10,``11,``12,``13,System.ValueTuple{``14,``15}}})")]
        [InlineData("M:System.Math.Max(System.SByte,System.SByte)")]
        [InlineData("M:System.MemoryExtensions.LastIndexOf(System.ReadOnlySpan{System.Char},System.ReadOnlySpan{System.Char},System.StringComparison)")]
        [InlineData("M:System.MemoryExtensions.TrimEnd(System.ReadOnlyMemory{System.Char})")]
        [InlineData("M:System.TupleExtensions.ToTuple``9(System.ValueTuple{``0,``1,``2,``3,``4,``5,``6,System.ValueTuple{``7,``8}})")]
        [InlineData("M:System.Delegate.DynamicInvoke(System.Object[])")]
        [InlineData("M:System.TupleExtensions.Deconstruct``19(System.Tuple{``0,``1,``2,``3,``4,``5,``6,System.Tuple{``7,``8,``9,``10,``11,``12,``13,System.Tuple{``14,``15,``16,``17,``18}}},``0@,``1@,``2@,``3@,``4@,``5@,``6@,``7@,``8@,``9@,``10@,``11@,``12@,``13@,``14@,``15@,``16@,``17@,``18@)")]
        [InlineData("M:System.Convert.TryToBase64Chars(System.ReadOnlySpan{System.Byte[][]},System.Span{System.Char},System.Int32@,System.Base64FormattingOptions)")]
        [InlineData("M:System.Buffer.MemoryCopy(System.Void*,System.Void***[]@,System.UInt64,System.UInt64)")]
        [InlineData("M:System.nuint.op_Implicit(System.Byte)")]
        [InlineData("M:System._AppDomain.DefineDynamicAssembly(System.Reflection.AssemblyName,System.Reflection.Emit.AssemblyBuilderAccess,System.String,System.Security.Policy.Evidence,System.Security.PermissionSet,System.Security.PermissionSet,System.Security.PermissionSet)")]
        [InlineData("M:System.Type.GetMethodImpl(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Reflection.CallingConventions,System.Type[],System.Reflection.ParameterModifier[])")]
        [InlineData("T:System.WeakReference`1")]
        [InlineData("M:System.Attribute.GetCustomAttributes(System.Reflection.Assembly,System.Type)")]
        [InlineData("P:System.MissingMemberException.MemberName")]
        [InlineData("M:System.WeakReference.#ctor")]
        [InlineData("F:System.MissingMemberException.MemberName")]
        [InlineData("M:System.Collections.Generic.List`1.BinarySearch(`0)")]
        [InlineData("E:System.MissingMemberException.MemberName")]
        [InlineData("M:System.TimeSpan.ToString")]
        public void AODNParser_ValidId_DoesNotThrowException(string id)
        {
            AODNTypeRequestParser parser = new AODNTypeRequestParser(id);
            var test = parser.ParseRequest().Syntax;
            Assert.Equal(test.ToString(), id);
        }
    }
}
