using System;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// The type of the delegate that will get called if <see cref="ISessionHelper.CommitAsync"/> is successful.
    /// </summary>
    public delegate Task AfterCommitSuccessDelegate();

    /// <summary>
    /// Represents a contract that provides helper methods for <see cref="ISession"/>.
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

        /// <summary>
        /// Registers an <see cref="AfterCommitSuccessDelegate"/> that will get called if <see cref="CommitAsync"/> is successful.
        /// </summary>
        void RegisterAfterCommit<T>(AfterCommitSuccessDelegate afterCommit);

        /// <summary>
        /// Commits the <see cref="ISession"/> and then if successful calls the registered <see cref="AfterCommitSuccessDelegate"/>.
        /// </summary>
        Task CommitAsync();
    }
}
