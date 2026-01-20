namespace OrchardCore.Indexing;

/// <summary>
/// An implementation of <see cref="IDocumentIndexHandler"/> can provide property values for an index document.
/// </summary>
public interface IDocumentIndexHandler
{
    Task BuildIndexAsync(BuildDocumentIndexContext context);
}
