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
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">A factory methods to load or create a document.</param>
        Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// For a full isolation, it needs to be used in pair with <see cref="LoadForUpdateAsync"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">A factory method to get or create a document for caching.</param>
        /// <returns></returns>
        Task<T> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new();
    }
}
