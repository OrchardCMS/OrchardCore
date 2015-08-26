using System;
using System.Collections.Generic;
using System.Reflection;
using Orchard.Hosting.Extensions.Models;

namespace Orchard.Hosting.Extensions {
    public class ExtensionEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public Assembly Assembly { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}