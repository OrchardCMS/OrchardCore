using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

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
        Assembly LoadAmbientExtension(IExtensionInfo extensionInfo);

        /// <summary>
        /// Loads an external precompiled extension.
        /// Returns an <see cref="Assembly"/> instance.
        /// </summary>
        Assembly LoadPrecompiledExtension(IExtensionInfo extensionInfo);

        /// <summary>
        /// Loads an external dynamic extension.
        /// Returns an <see cref="Assembly"/> instance.
        /// </summary>
        Assembly LoadDynamicExtension(IExtensionInfo extensionInfo);

        /// <summary>
        /// Lists references of all the available extensions.
        /// Returns <see cref="MetadataReference"/> instances.
        /// </summary>
        IEnumerable<string> MetadataPaths { get; }

        /// <summary>
        /// Lists all assemblies dynamically loaded.
        /// Returns <see cref="Assembly"/> instances.
        /// </summary>
        IEnumerable<Assembly> LoadedAssemblies { get; }
    }
}