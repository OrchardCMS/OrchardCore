using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orchard.Environment.Extensions.Info.Extensions.Loaders
{
    public class ExtensionEntry
    {
        public IExtensionInfo ExtensionInfo { get; set; }
        public Assembly Assembly { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}
