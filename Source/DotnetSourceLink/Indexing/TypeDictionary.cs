using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSourceLink.Indexing
{
    internal sealed class TypeDictionary
    {
        private readonly Dictionary<string, List<TypeNode>> _classDictionary =
            new Dictionary<string, List<TypeNode>>();

        public void AddType(TypeNode type)
        {
            string name = type.Identifier;
            bool containsElement = _classDictionary.ContainsKey(name);

            if (!containsElement)
            {
                _classDictionary.Add(name, new List<TypeNode>(capacity: 1));
                _classDictionary[name].Add(type);
                return;
            }

            var typeList = _classDictionary[name];

            if (!type.IsPartial)
            {
                typeList.Add(type);
            }

            else
            {
                switch (typeList.Count(x => x.IsPartial))
                {
                    case 0:
                    {
                        typeList.Add(type);
                    } return;
                    case 1:
                    {
                        typeList.Single(x => x.IsPartial).AddType(type);
                    } return;

                    default: { throw new ArgumentException(); }
                }
            }
        }

        public IEnumerable<string> GetLocation(string type)
        {
            return _classDictionary[type]
                .Select(x => string.Join(',', x.Locations.Select(y => y.File.ToString())));
        }
    }
}
