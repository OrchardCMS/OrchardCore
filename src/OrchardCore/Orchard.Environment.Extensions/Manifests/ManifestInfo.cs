using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Manifests
{
    public class ManifestInfo : IManifestInfo
    {
        private readonly IConfigurationRoot _configurationRoot;
        private string _type;
        private Lazy<IEnumerable<string>> _tags;
        private Lazy<Version> _version;

        public ManifestInfo
        (
            IConfigurationRoot configurationRoot,
            string type
        )
        {
            _configurationRoot = configurationRoot;
            _type = type;
            _tags = new Lazy<IEnumerable<string>>(ParseTags);
            _version = new Lazy<Version>(ParseVersion);
        }

        public bool Exists => true;
        public string Name => _configurationRoot["name"];
        public string Description => _configurationRoot["description"];
        public string Type => _type;
        public string Author => _configurationRoot["author"];
        public string Website => _configurationRoot["website"];
        public Version Version => _version.Value;
        public IEnumerable<string> Tags => _tags.Value;
        public IConfigurationRoot ConfigurationRoot => _configurationRoot;

        private IEnumerable<string> ParseTags()
        {
            var tags = _configurationRoot["tags"];

            if (string.IsNullOrWhiteSpace(tags))
                return Enumerable.Empty<string>();

            return tags.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private Version ParseVersion()
        {
            var value = _configurationRoot["version"];

            if (string.IsNullOrWhiteSpace(value))
                return new Version(0, 0);

            Version version;
            Version.TryParse(value, out version);
            return version;
        }
    }
}
