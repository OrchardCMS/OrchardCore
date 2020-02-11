using System.Threading.Tasks;

namespace OrchardCore.Data
{
    /// <summary>
    /// The type of the delegate that will get called before / after <see cref="ICommittableDataStore.CommitAsync"/>.
    /// </summary>
    public delegate Task DataStoreCommitDelegate();
}
