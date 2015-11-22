using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions
{
    public class ExtensionLoadingContext
    {
        public ExtensionLoadingContext()
        {
            ProcessedExtensions = new Dictionary<string, ExtensionProbeEntry>(StringComparer.OrdinalIgnoreCase);
            ProcessedReferences = new Dictionary<string, ExtensionReferenceProbeEntry>(StringComparer.OrdinalIgnoreCase);
            DeleteActions = new List<Action>();
            CopyActions = new List<Action>();
        }

        public IDictionary<string, ExtensionProbeEntry> ProcessedExtensions { get; private set; }
        public IDictionary<string, ExtensionReferenceProbeEntry> ProcessedReferences { get; private set; }

        public IList<Action> DeleteActions { get; private set; }
        public IList<Action> CopyActions { get; private set; }

        /// <summary>
        /// Keep track of modification date of files (VirtualPath => DateTime)
        /// </summary>
        public ConcurrentDictionary<string, DateTime> VirtualPathModficationDates { get; set; }

        /// <summary>
        /// List of extensions (modules) present in the system
        /// </summary>
        public List<ExtensionDescriptor> AvailableExtensions { get; set; }

        /// <summary>
        /// For every extension name, the list of loaders that can potentially load
        /// that extension (in order of "best-of" applicable)
        /// </summary>
        public IDictionary<string, IEnumerable<ExtensionProbeEntry>> AvailableExtensionsProbes { get; set; }

        /// <summary>
        /// For every reference name, list of potential loaders/locations
        /// </summary>
        public IDictionary<string, IEnumerable<ExtensionReferenceProbeEntry>> ReferencesByModule { get; set; }

        /// <summary>
        /// For every extension name, list of potential loaders/locations
        /// </summary>
        public IDictionary<string, IEnumerable<ExtensionReferenceProbeEntry>> ReferencesByName { get; set; }
    }
}