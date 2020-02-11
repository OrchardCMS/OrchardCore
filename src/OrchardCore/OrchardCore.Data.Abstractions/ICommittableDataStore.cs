using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public interface ICommittableDataStore
    {
        /// <summary>
        /// Allow to cancel the scoped transaction before calling <see cref="CommitAsync"/>.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Registers a <see cref="DataStoreCommitDelegate"/> that will get called before <see cref="CommitAsync"/>.
        /// </summary>
        void BeforeCommit<T>(DataStoreCommitDelegate beforeCommit);

        /// <summary>
        /// Registers a <see cref="DataStoreCommitDelegate"/> that will get called after <see cref="CommitAsync"/> if it is successful.
        /// </summary>
        void AfterCommitSuccess<T>(DataStoreCommitDelegate afterCommit);

        /// <summary>
        /// Registers a <see cref="DataStoreCommitDelegate"/> that will get called after <see cref="CommitAsync"/> even it is not successful
        /// </summary>
        void AfterCommit<T>(DataStoreCommitDelegate afterCommit);

        /// <summary>
        /// Calls the related <see cref="DataStoreCommitDelegate"/> before and after committing the data store/>.
        /// </summary>
        Task CommitAsync();
    }
}
