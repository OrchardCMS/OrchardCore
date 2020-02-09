using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Provides helper methods for <see cref="ISession"/>.
    /// </summary>
    public interface ISessionHelper : ICacheableDataStore
    {
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
