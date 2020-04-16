using OrchardCore.Data.Documents;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptions<TDocument> where TDocument : IDocument, new()
    {
        public DocumentOptions(IDocumentOptionsFactory factory)
        {
            Value = factory.Create(typeof(TDocument));
        }

        public DocumentOptions Value { get; }
    }
}
