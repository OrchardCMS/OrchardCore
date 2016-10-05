using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info
{
    public interface IManifestInfo
    {
        IFileInfo Manifest { get; }
        bool Exists { get; }
        string Name { get; }
        string Description { get; }
        IDictionary<string, string> Attributes { get; }
    }
}
