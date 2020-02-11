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
        /// Registers a <see cref="CommitDelegate"/> that will get called before <see cref="CommitAsync"/>.
        /// </summary>
        void BeforeCommit<T>(CommitDelegate beforeCommit);

        /// <summary>
        /// Registers a <see cref="CommitDelegate"/> that will get called after <see cref="CommitAsync"/> if it is successful.
        /// </summary>
        void AfterCommitSuccess<T>(CommitDelegate afterCommit);

        /// <summary>
        /// Registers a <see cref="CommitDelegate"/> that will get called after <see cref="CommitAsync"/> even it is not successful
        /// </summary>
        void AfterCommit<T>(CommitDelegate afterCommit);

        /// <summary>
        /// Calls the related <see cref="CommitDelegate"/> before and after committing the data store/>.
        /// </summary>
        Task CommitAsync();
    }
}
