using System.Collections.Generic;

namespace Orchard.Scripting
{
    public interface IGlobalMethodProvider
    {
        IEnumerable<GlobalMethod> GetMethods();
    }
}

