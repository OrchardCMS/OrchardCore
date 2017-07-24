using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Orchard.Mvc
{
    /// <summary>
    /// A provider for a given <typeparamref name="TFeature"/> feature.
    /// </summary>
    /// <typeparam name="TFeature">The type of the feature.</typeparam>
    public interface IShellFeatureProvider<TFeature> : IApplicationFeatureProvider<TFeature>
    {
        /// <summary>
        /// Updates the <paramref name="feature"/> instance.
        /// </summary>
        /// <param name="parts">The list of <see cref="ApplicationPart"/>s of the
        /// application.
        /// </param>
        /// <param name="feature">The feature instance to populate.</param>
        void PopulateShellFeature(IEnumerable<ApplicationPart> parts, TFeature feature);
    }
}
