using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement
{
    public class ContentIdentity
    {
        private readonly Dictionary<string, string> _dictionary;

        public ContentIdentity()
        {
            _dictionary = new Dictionary<string, string>();
        }

        public ContentIdentity(string identity)
        {
            _dictionary = new Dictionary<string, string>();
            if (!String.IsNullOrEmpty(identity))
            {
                var identityEntries = GetIdentityEntries(identity);
                foreach (var identityEntry in identityEntries)
                {
                    var keyValuePair = GetIdentityKeyValue(identityEntry);
                    if (keyValuePair != null)
                    {
                        _dictionary.Add(keyValuePair.Value.Key, UnencodeIdentityValue(keyValuePair.Value.Value));
                    }
                }
            }
        }

        public void Add(string name, string value)
        {
            if (_dictionary.ContainsKey(name))
            {
                _dictionary[name] = value;
            }
            else {
                _dictionary.Add(name, value);
            }
        }

        public string Get(string name)
        {
            return Has(name) ? _dictionary[name] : null;
        }

        public bool Has(string name)
        {
            return _dictionary.ContainsKey(name);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var key in _dictionary.Keys.OrderBy(key => key))
            {
                var escapedIdentity = EncodeIdentityValue(_dictionary[key]);
                stringBuilder.Append("/" + key + "=" + escapedIdentity);
            }
            return stringBuilder.ToString();
        }

        private static string EncodeIdentityValue(string identityValue)
        {
            if (String.IsNullOrEmpty(identityValue))
            {
                return "";
            }

            var stringBuilder = new StringBuilder();
            foreach (var ch in identityValue.ToCharArray())
            {
                switch (ch)
                {
                    case '\\':
                        stringBuilder.Append('\\');
                        stringBuilder.Append('\\');
                        break;
                    case '/':
                        stringBuilder.Append('\\');
                        stringBuilder.Append('/');
                        break;
                    default:
                        stringBuilder.Append(ch);
                        break;
                }
            }
            return stringBuilder.ToString();
        }

        private static string UnencodeIdentityValue(string identityValue)
        {
            var stringBuilder = new StringBuilder();
            var identityChars = identityValue.ToCharArray();
            var length = identityChars.Length;
            for (int i = 0; i < length; i++)
            {
                switch (identityChars[i])
                {
                    case '\\':
                        if (i + 1 < length)
                        {
                            if (identityChars[i + 1] == '\\')
                            {
                                stringBuilder.Append('\\');
                                i++;
                            }
                        }
                        else {
                            stringBuilder.Append('\\');
                        }
                        break;
                    default:
                        stringBuilder.Append(identityChars[i]);
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private static IEnumerable<string> GetIdentityEntries(string identity)
        {
            var identityEntries = new List<string>();
            var stringBuilder = new StringBuilder();
            var escaping = false;
            foreach (var ch in identity.ToCharArray())
            {
                if (escaping)
                {
                    stringBuilder.Append(ch);
                    escaping = false;
                }
                else {
                    if (ch == '/')
                    {
                        if (stringBuilder.Length > 0)
                        {
                            identityEntries.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                        }
                        stringBuilder.Append(ch);
                    }
                    else {
                        if (ch == '\\')
                        {
                            escaping = true;
                        }
                        stringBuilder.Append(ch);
                    }
                }
            }
            identityEntries.Add(stringBuilder.ToString());

            return identityEntries;
        }

        private static KeyValuePair<string, string>? GetIdentityKeyValue(string identityEntry)
        {
            if (String.IsNullOrWhiteSpace(identityEntry)) return null;
            if (!identityEntry.StartsWith("/")) return null;
            var indexOfEquals = identityEntry.IndexOf("=");
            if (indexOfEquals < 0) return null;

            var key = identityEntry.Substring(1, indexOfEquals - 1);
            var value = identityEntry.Substring(indexOfEquals + 1);

            return new KeyValuePair<string, string>(key, value);
        }


        public class ContentIdentityEqualityComparer : IEqualityComparer<ContentIdentity>
        {
            public static bool AreEquivalent(ContentIdentity contentIdentity1, ContentIdentity contentIdentity2)
            {
                if (contentIdentity1 == null) return contentIdentity2 == null;
                if (contentIdentity2 == null) return false;
                return contentIdentity1._dictionary.Keys
                    .Any(k =>
                    {
                        var other = contentIdentity2.Get(k);
                        if (other == null) return false;
                        return contentIdentity1._dictionary[k] == other;
                    });
            }

            public bool Equals(ContentIdentity contentIdentity1, ContentIdentity contentIdentity2)
            {
                if (contentIdentity1._dictionary.Keys.Count != contentIdentity2._dictionary.Keys.Count)
                    return false;

                foreach (var key in contentIdentity1._dictionary.Keys)
                {
                    if (!contentIdentity2._dictionary.ContainsKey(key))
                    {
                        return false;
                    }

                    if (contentIdentity2._dictionary[key] != contentIdentity1._dictionary[key])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(ContentIdentity contentIdentity)
            {
                return contentIdentity._dictionary.OrderBy(kvp => kvp.Key).ToString().GetHashCode();
            }
        }

    }
}

