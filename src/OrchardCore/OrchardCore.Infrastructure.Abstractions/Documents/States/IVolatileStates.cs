namespace OrchardCore.Documents.States
{
    /// <summary>
    /// Shares tenant level states that are cached but not persisted.
    /// </summary>
    public interface IVolatileStates : IDocumentStates
    {
    }
}
