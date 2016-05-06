using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions
{
    /// <summary>
    /// Provides extension library utilities.
    /// Its lifetime is a host level singleton.
    /// </summary>
    public interface IExtensionLibraryService
    {
        /// <summary>
        /// Loads the project library of a precompiled extension.
        /// Returns an <see cref="Assembly"/> instance.
        /// </summary>
        Assembly LoadProject(ExtensionDescriptor descriptor);

        /// <summary>
        /// Lists metadata references of all the available extensions.
        /// Returns <see cref="MetadataReference"/> instances.
        /// </summary>
        IEnumerable<MetadataReference> MetadataReferences();
    }
}