namespace DotnetSourceLink.Indexing
{
    public struct MemberLocation
    {
        public SourceFile File { get; }
        public ushort StartLineNumber { get; }
        public ushort EndLineNumber { get; }

        public MemberLocation(Repository repository, string file, (ushort start, ushort end) line)
        {
            File = new SourceFile((repository, file));
            StartLineNumber = line.start;
            EndLineNumber = line.end;
        }

        public override string ToString() => $"{File}#L{StartLineNumber}-L{EndLineNumber}";
    }

    public enum Repository : byte
    {
        CoreFx,
        CoreClr
    }
}
