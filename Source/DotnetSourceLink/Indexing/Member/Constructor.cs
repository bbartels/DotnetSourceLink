using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSourceLink.Indexing.Member
{
    internal class Constructor
    {
        public string Name { get; }
        public string[] Parameters { get; }

        public string Identifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Name).Append('(');

                if (Parameters.Length > 0)
                {
                    foreach (var param in Parameters)
                    {
                        sb.Append(param).Append(',');
                    }

                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.Append(')').ToString();
            }
        }

        public MemberLocation Location { get; }

        public Constructor(string name, string[] parameters, MemberLocation location)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}
