using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Reprensents the list of logical identities of a <see cref="ContentItem"/>.
    /// </summary>
    [JsonConverter(typeof(ContentIdentityConverter))]
    public class ContentIdentity : IEquatable<ContentIdentity>
    {
        private IDictionary<string, string> _identities;

        public ContentIdentity()
        {
        }

        public void Add(string name, string value)
        {
            if (_identities == null)
            {
                _identities = new Dictionary<string, string>();
            }

            _identities[name] = value;
        }

        public string Get(string name)
        {
            string value = null;
            if (_identities != null && _identities.TryGetValue(name, out value))
            {
                return value;
            }

            return null;
        }

        public bool Has(string name)
        {
            return _identities != null && _identities.ContainsKey(name);
        }

        public IEnumerable<string> Names
        {
            get
            {
                return _identities == null ? Enumerable.Empty<string>() : _identities.Keys;
            }
        }

        public bool Equals(ContentIdentity other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (_identities == null || other._identities == null)
            {
                return _identities == null && other._identities == null;
            }

            foreach(var identity in _identities)
            {
                string otherValue = null;
                if (other._identities.TryGetValue(identity.Key, out otherValue))
                {
                    return identity.Value == otherValue;
                }
            }

            return false;
        }
    }
}

