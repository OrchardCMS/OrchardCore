namespace OrchardCore.Documents.Options
{
    public interface IDocumentNamedOptions
    {
        string CacheKey { get; set; }
        string CacheIdKey { get; set; }
    }

    public class DocumentNamedOptions : DocumentOptionsBase, IDocumentNamedOptions
    {
        public string CacheKey { get; set; }
        public string CacheIdKey { get; set; }
    }
}
