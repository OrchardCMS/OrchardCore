namespace OrchardCore.Documents.States
{
    /// <summary>
    /// Shares tenant level states that are cached and persisted.
    /// </summary>
    public class PersistentStates : DocumentStates<PersistentDocument>, IPersistentStates
    {
        public PersistentStates(IVolatileDocumentManager<PersistentDocument> documentManager) : base(documentManager)
        {
        }
    }

    public class PersistentDocument : DocumentEntity
    {
    }
}
