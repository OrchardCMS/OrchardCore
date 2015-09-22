using Orchard.Environment.Extensions.Models;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    /// <summary>
    /// An Extension Manager implementation provides a way to load features and their types.
    /// Its lifetime is a host level singleton.
    /// </summary>
    public interface IExtensionManager
    {
        /// <summary>
        /// Lists all the available extensions. This method doesn't load the extensions,
        /// but instead returns <see cref="ExtensionDescriptor"/> instances.
        /// </summary>
        IEnumerable<ExtensionDescriptor> AvailableExtensions();

        /// <summary>
        /// Lists all the available features ordered by dependency and priority. This
        /// method doesn't load the extensions, but instead returns <see cref="FeatureDescriptor"/>
        /// instances.
        /// </summary>
        IEnumerable<FeatureDescriptor> AvailableFeatures();

        /// <summary>
        /// Returns an extension from its id. This method doesn't load the extensions,
        /// but instead returns the <see cref="ExtensionDescriptor"/> instance.
        /// </summary>
        /// <returns><c>null</c> if the extension doesn't exist.</returns>
        ExtensionDescriptor GetExtension(string id);

        /// <summary>
        /// Loads the assembly of the specified features and their types.
        /// </summary>
        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);
        bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject);
    }
}