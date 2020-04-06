using MessagePack;
using OrchardCore.Data.Documents;
using OrchardCore.Entities;

namespace OrchardCore.Documents
{
    /// <summary>
    /// An <see cref="IDocument"/> being an <see cref="IEntity"/> that is serializable by 'MessagePack'.
    /// </summary>
    public interface IDocumentEntity : IDocument, IEntity, IMessagePackSerializationCallbackReceiver
    {
        string JsonProperties { get; set; }
    }
}
