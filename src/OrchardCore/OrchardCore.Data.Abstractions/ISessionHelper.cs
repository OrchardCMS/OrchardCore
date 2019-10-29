using System;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public interface ISessionHelper
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// For a full isolation, it needs to be used in pair with <see cref="GetForCachingAsync"/>.
        /// </summary>
        Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// For a full isolation, it needs to be used in pair with <see cref="LoadForUpdateAsync"/>.
        /// </summary>
        Task<T> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new();
    }
}
