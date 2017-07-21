using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
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