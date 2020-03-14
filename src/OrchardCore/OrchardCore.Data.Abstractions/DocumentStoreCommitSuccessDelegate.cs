using System.Threading.Tasks;

namespace OrchardCore.Data
{
    /// <summary>
    /// The type of the delegate that will get called after <see cref="ICommittableDocumentStore.CommitAsync"/> if it is successful.
    /// </summary>
    public delegate Task DocumentStoreCommitSuccessDelegate();
}
