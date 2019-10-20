namespace DotnetSourceLink.Misc
{
    internal static class IdentifierComparator
    {
        //TODO: Remove??
        public static bool Compare(string identifier1, string identifier2)
        {
            var idEnum1 = new IdentifierEnumerator(identifier1, true);
            var idEnum2 = new IdentifierEnumerator(identifier2, true);

            while (idEnum1.MoveNext() == idEnum2.MoveNext())
            {
                if (!idEnum1.Current.Equals(idEnum2.Current)) { return false; }
            }

            return true;
        }
    }
}
