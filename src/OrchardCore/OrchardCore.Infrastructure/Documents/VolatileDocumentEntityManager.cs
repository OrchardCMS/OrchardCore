namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="DocumentEntityManager{TDocumentEntity}"/> using a multi level cache but without any persistent storage.
    /// </summary>
    public class VolatileDocumentEntityManager<TDocumentEntity> : DocumentEntityManager<TDocumentEntity>, IVolatileDocumentEntityManager<TDocumentEntity> where TDocumentEntity : class, IDocumentEntity, new()
    {
        public VolatileDocumentEntityManager(IVolatileDocumentManager<TDocumentEntity> documentManager) : base(documentManager)
        {
        }
    }
}
