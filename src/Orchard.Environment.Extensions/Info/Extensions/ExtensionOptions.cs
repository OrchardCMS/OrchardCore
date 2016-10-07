using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info.Extensions
{
    public class ExtensionOptions
    {
        public IList<string> SearchPaths { get; }
            = new List<string>();
    }
}