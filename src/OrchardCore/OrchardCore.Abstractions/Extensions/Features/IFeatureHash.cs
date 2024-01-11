using System.Threading.Tasks;

namespace OrchardCore.Environment.Extensions.Features
{
    /// <summary>
    /// An implementation of this interface provide efficient access to the state
    /// of the enabled feature in order to provide hashes used for caching.
    /// Because its state should be cached, the instance should not have any state
    /// thus is declared as transient.
    /// </summary>
    public interface IFeatureHash
    {
        /// <summary>
        /// Returns a serial number representing the list of available features for the current tenant.
        /// </summary>
        /// <returns>
        /// An <see cref="int"/> value that changes every time the list of features changes.
        /// The implementation is efficient in order to be called frequently.
        /// </returns>
        Task<int> GetFeatureHashAsync();

        /// <summary>
        /// Returns a serial number representing the list of available features for the current tenant.
        /// </summary>
        /// <returns>
        /// A <see cref="int"/> value that changes every time a specific feature is enabled.
        /// The implementation is efficient in order to be called frequently.
        /// </returns>
        Task<int> GetFeatureHashAsync(string featureId);
    }
}
