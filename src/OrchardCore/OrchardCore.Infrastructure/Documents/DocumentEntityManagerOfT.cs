using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="DocumentEntityManager{TDocumentEntity}"/> using a given type of <see cref="IDocumentStore"/>.
    /// </summary>
    public class DocumentEntityManager<TDocumentStore, TDocumentEntity> : DocumentEntityManager<TDocumentEntity>, IDocumentEntityManager<TDocumentStore, TDocumentEntity>
        where TDocumentStore : IDocumentStore where TDocumentEntity : class, IDocumentEntity, new()
    {
        public DocumentEntityManager(IDocumentManager<TDocumentEntity> documentManager) : base(documentManager)
        {
        }
    }
}
