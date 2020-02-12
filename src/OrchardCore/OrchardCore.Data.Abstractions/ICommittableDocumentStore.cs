using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public interface ICommittableDocumentStore
    {
        /// <summary>
        /// Allows to cancel the current scoped updating before calling <see cref="CommitAsync"/>.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitDelegate"/> that will get called before <see cref="CommitAsync"/>.
        /// </summary>
        void BeforeCommit<T>(DocumentStoreCommitDelegate beforeCommit);

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitDelegate"/> that will get called after <see cref="CommitAsync"/> if it is successful.
        /// </summary>
        void AfterCommitSuccess<T>(DocumentStoreCommitDelegate afterCommit);

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitDelegate"/> that will get called after <see cref="CommitAsync"/> even it is not successful
        /// </summary>
        void AfterCommit<T>(DocumentStoreCommitDelegate afterCommit);

        /// <summary>
        /// Calls the related <see cref="DocumentStoreCommitDelegate"/> before and after committing the data store/>.
        /// </summary>
        Task CommitAsync();
    }
}
