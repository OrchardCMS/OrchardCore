using System.Threading.Tasks;

namespace OrchardCore.Data
{
    /// <summary>
    /// The type of the delegate that will get called before / after <see cref="ICommittableDocumentStore.CommitAsync"/>.
    /// </summary>
    public delegate Task DocumentStoreCommitDelegate();
}
