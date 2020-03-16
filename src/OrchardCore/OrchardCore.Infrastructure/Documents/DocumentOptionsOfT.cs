using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    public class DocumentOptions<TDocument> where TDocument : BaseDocument, new()
    {
        public DocumentOptions(IDocumentOptionsFactory factory)
        {
            Value = factory.Create(typeof(TDocument));
        }

        public DocumentOptions Value { get; }
    }
}
