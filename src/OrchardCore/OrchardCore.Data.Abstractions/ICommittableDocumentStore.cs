using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public interface ICommittableDocumentStore
    {
        /// <summary>
        /// Allows to cancel the current updating before calling <see cref="CommitAsync"/> at the end of the scope.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitSuccessDelegate"/> that will get called after <see cref="CommitAsync"/> if it succeeded.
        /// </summary>
        void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommit);

        /// <summary>
        /// Registers a <see cref="DocumentStoreCommitFailureDelegate"/> that will get called after <see cref="CommitAsync"/> if it failed.
        /// </summary>
        void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommit);

        /// <summary>
        /// Commits the related document store/>.
        /// </summary>
        Task CommitAsync();
    }
}
