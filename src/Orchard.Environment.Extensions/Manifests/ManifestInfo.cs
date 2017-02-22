using System;
using System.Collections;
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

        public ManifestInfo
        (
            IConfigurationRoot configurationRoot,
            string type
        )
        {
            _configurationRoot = configurationRoot;
            _type = type;
            _tags = new Lazy<IEnumerable<string>>(ParseTags);
        }

        public bool Exists => true;
        public string Name => _configurationRoot["name"];
        public string Description => _configurationRoot["description"];
        public string Type => _type;
        public IEnumerable<string> Tags => _tags.Value;
        public IConfigurationRoot ConfigurationRoot => _configurationRoot;

        private IEnumerable<string> ParseTags()
        {
            var tags = _configurationRoot["tags"];

            if (string.IsNullOrWhiteSpace(tags))
                return Enumerable.Empty<string>();

            return tags.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
    }
}
