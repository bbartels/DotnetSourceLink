using System.Collections.Generic;

namespace DotnetSourceLink.Indexing
{
    public readonly struct SourceFile
    {
        private static readonly FileLocationCache FileLocationCache = new FileLocationCache();
        private static readonly Dictionary<Repository, string> RepositoryDictionary = new Dictionary<Repository, string>()
        {
            { Repository.CoreFx, "dotnet/corefx" },
            { Repository.CoreClr, "dotnet/coreclr" },
        };
        private readonly ushort _fileId;

        public Repository Repository => FileLocationCache[_fileId].repository;
        public string Path => FileLocationCache[_fileId].path;
        public (Repository repository, string path) Location => FileLocationCache[_fileId];

        public SourceFile((Repository repository, string path) file)
        {
            _fileId = FileLocationCache.GetOrAddFile(file);
        }

        internal SourceFile(ushort fileId)
        {
            _fileId = fileId;
        }

        public override string ToString()
        {
            var (repository, path) = Location;

            return RepositoryDictionary[repository] + "/release/2.2/" + (path[0] == '/' ? path.Substring(1) : path);
        }
    }
}
