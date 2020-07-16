using System;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    /// <summary>
    /// Represents a contract that provides helper methods for <see cref="YesSql.ISession"/>.
    /// </summary>
    public interface ISessionHelper
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// For a full isolation, it needs to be used in pair with <see cref="GetForCachingAsync"/>.
        /// </summary>
        Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated,
        /// and specifies if the returned document can be cached or not if it has been already loaded.
        /// For a full isolation, it needs to be used in pair with <see cref="LoadForUpdateAsync"/>.
        /// </summary>
        Task<(bool, T)> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new();
    }
}
