using OrchardCore.Data.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Documents
{
    /// <summary>
    /// An <see cref="IDocument"/> being an <see cref="IEntity"/>.
    /// </summary>
    public interface IDocumentEntity : IDocument, IEntity
    {
    }
}
