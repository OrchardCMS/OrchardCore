using Microsoft.Framework.Configuration;
using System;
using System.Collections.Generic;

namespace Orchard.Environment.Shell {
    internal class InternalConfigurationSource : IConfigurationSource {
        private readonly IDictionary<string, string> _values;

        public InternalConfigurationSource() {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Load() {
            throw new NotSupportedException();
        }

        public IEnumerable<string> ProduceConfigurationSections(IEnumerable<string> earlierKeys, string prefix, string delimiter) {
            throw new NotImplementedException();
        }

        public void Set(string key, string value) {
            _values.Add(key, value);
        }

        public bool TryGet(string key, out string value) {
            return _values.TryGetValue(key, out value);
        }
    }
}
