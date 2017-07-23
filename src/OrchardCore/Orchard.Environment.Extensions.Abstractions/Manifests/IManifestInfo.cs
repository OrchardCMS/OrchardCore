using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions
{
    public interface IManifestInfo
    {
        bool Exists { get; }
        string Name { get; }
        string Description { get; }
        string Type { get; }
        string Author { get; }
        string Website { get; }
        Version Version { get; }
        IEnumerable<string> Tags { get; }
        IConfigurationRoot ConfigurationRoot { get; }
    }
}