namespace OrchardCore.Documents
{
    /// <summary>
    /// An <see cref="IDocumentEntityManager{TDocumentEntity}"/> using a shared cache but without any persistent storage.
    /// </summary>
    public interface IVolatileDocumentEntityManager<TDocumentEntity> : IDocumentEntityManager<TDocumentEntity> where TDocumentEntity : class, IDocumentEntity, new()
    {
    }
}
