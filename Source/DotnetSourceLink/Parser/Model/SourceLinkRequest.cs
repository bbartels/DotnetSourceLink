namespace DotnetSourceLink.Parser.Model
{
    public sealed class SourceElementRequest
    {
        public ISyntax Syntax { get; }

        public SourceElementRequest(ISyntax syntax)
        {
            Syntax = syntax;
        }
    }
}
