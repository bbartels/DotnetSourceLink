using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

using DotnetSourceLink.Indexing;
using DotnetSourceLink.Indexing.Member;
using DotnetSourceLink.Parser;

namespace DotnetSourceLink
{
    public class RepositoryManager
    {
        public string BasePath { get; }
        public Repository Repository { get; }

        private readonly SourceElementManager _elementManager = new SourceElementManager(new NamespaceNode(null, null));
        private readonly RepositoryParser _repositoryParser;
        //private const string BasePath = @"H:\dotnet\corefx";
        //private const string BasePath = @"/home/corefx/";

        public RepositoryManager(Repository repo, string basePath)
        {
            Repository = repo;
            BasePath = basePath;
            _repositoryParser = new RepositoryParser(_elementManager, Repository);
        }

        public async Task Scan()
        {
            await EnumerateDirectories(BasePath);
            await Task.Run(() => _repositoryParser.Scan());
        }

        private async Task EnumerateDirectories(string sDir)
        {
            var fileProcessTasks = Directory.EnumerateFiles(sDir, "*.cs", SearchOption.AllDirectories)
                .Where(FilterFile)
                .Select(ParseFile);

            static bool FilterFile(string file)
            {
                return Path.GetExtension(file.AsSpan()).Equals(".cs", StringComparison.Ordinal) &&
                       !file.Contains("tests", StringComparison.InvariantCulture) &&
                       Regex.Matches(file, "src").Count == 2;
            }

            await Task.WhenAll(fileProcessTasks);
        }

        public (IEnumerable<MemberLocation>, string message) Get(string id)
        {
            try
            {
                AODNTypeRequestParser parser = new AODNTypeRequestParser(id);
                var request = parser.ParseRequest();
                var test = _elementManager.FindElement(request);

                return (test switch
                {
                    TypeNode type => type.Locations,
                    AbstractMember method => new [] { method.Location },
                    _ => Enumerable.Empty<MemberLocation>()
                }, "Success");
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }

        private async Task ParseFile(string file)
        {
            string source;
            using (StreamReader sr = File.OpenText(file))
            {
                source = await sr.ReadToEndAsync();
            }

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            _ = _repositoryParser.ParseNode(syntaxTree.GetRoot(), TrimStart(file, BasePath));
        }

        private string TrimStart(string str, string prefix)
            => (str.StartsWith(prefix) ? str.Remove(0, prefix.Length) : str).Replace('\\', '/');
    }
}
