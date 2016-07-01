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
        /// Loads an ambient extension.
        /// Returns an <see cref="Assembly"/> instance.
        /// </summary>
        Assembly LoadAmbientExtension(ExtensionDescriptor descriptor);

        /// <summary>
        /// Loads an external precompiled extension.
        /// Returns an <see cref="Assembly"/> instance.
        /// </summary>
        Assembly LoadPrecompiledExtension(ExtensionDescriptor descriptor);

        /// <summary>
        /// Loads an external dynamic extension.
        /// Returns an <see cref="Assembly"/> instance.
        /// </summary>
        Assembly LoadDynamicExtension(ExtensionDescriptor descriptor);

        /// <summary>
        /// Lists references of all the available extensions.
        /// Returns <see cref="MetadataReference"/> instances.
        /// </summary>
        IEnumerable<MetadataReference> MetadataReferences();
    }
}