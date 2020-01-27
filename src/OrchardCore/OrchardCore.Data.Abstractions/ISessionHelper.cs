using System;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Represents a contract that provides helper methods for <see cref="ISession"/>.
    /// </summary>
    public interface ISessionHelper
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// For a full isolation, it needs to be used in pair with <see cref="GetForCachingAsync"/>.
        Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// For a full isolation, it needs to be used in pair with <see cref="LoadForUpdateAsync"/>.
        /// </summary>
        Task<T> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new();

        /// <summary>
        /// Registers a <see cref="CommitDelegate"/> that will get called before <see cref="CommitAsync"/>.
        /// </summary>
        void RegisterBeforeCommit<T>(CommitDelegate beforeCommit);

        /// <summary>
        /// Registers a <see cref="CommitDelegate"/> that will get called after <see cref="CommitAsync"/> if it is successful.
        /// </summary>
        void RegisterAfterCommitSuccess<T>(CommitDelegate afterCommit);

        /// <summary>
        /// Registers a <see cref="CommitDelegate"/> that will get called after <see cref="CommitAsync"/> even it is not successful
        /// </summary>
        void RegisterAfterCommit<T>(CommitDelegate afterCommit);

        /// <summary>
        /// Calls the related <see cref="CommitDelegate"/> before and after committing the <see cref="ISession"/>.
        /// </summary>
        Task CommitAsync();
    }
}
