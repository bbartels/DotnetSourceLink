using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetSourceLink.Misc
{
    interface IGenericTypeOffsetLocator
    {
        (byte depth, byte index)? GetTypeParameterDepth(string identifier);
    }
}
