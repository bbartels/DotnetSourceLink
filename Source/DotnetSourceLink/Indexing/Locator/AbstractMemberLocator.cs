using DotnetSourceLink.Indexing.Member;

namespace DotnetSourceLink.Indexing.Locator
{
    internal abstract class AbstractMemberLocator
    {
        protected readonly AbstractNode _root;

        protected AbstractMemberLocator(AbstractNode root)
        {
            _root = root;
        }

        public abstract ISourceElement Locate();
    }
}
