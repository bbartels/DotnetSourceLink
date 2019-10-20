using DotnetSourceLink.Misc;

namespace DotnetSourceLink.Indexing
{
    internal sealed class FileLocationCache
    {
        private readonly ConcurrentBictionary<ushort, (Repository repository, string path)> _fileDictionary
            = new ConcurrentBictionary<ushort, (Repository repository, string path)>();
        private ushort _currentKey;

        public ushort GetOrAddFile((Repository repository, string path) file)
        {
            if (_fileDictionary.ContainsKey(file)) { return _fileDictionary[file]; }

            _fileDictionary.Add(_currentKey, file);
            return _currentKey++;
        }

        public (Repository repository, string path) this[ushort key] => _fileDictionary[key];
        public ushort this[(Repository, string) file] => _fileDictionary[file];
    }
}
