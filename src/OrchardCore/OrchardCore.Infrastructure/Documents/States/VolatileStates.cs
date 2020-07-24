namespace OrchardCore.Documents.States
{
    /// <summary>
    /// Shares tenant level states that are cached but not persisted.
    /// </summary>
    public class VolatileStates : DocumentStates<VolatileDocument>, IVolatileStates
    {
        public VolatileStates(IVolatileDocumentManager<VolatileDocument> documentManager) : base(documentManager)
        {
        }
    }

    public class VolatileDocument : DocumentEntity
    {
    }
}
