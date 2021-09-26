using System;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptions : DocumentOptionsBase, IDocumentNamedOptions, IDocumentSharedOptions
    {
        // Only from the named config or default.
        public string CacheKey { get; set; }
        public string CacheIdKey { get; set; }

        // Only from the shared config or default.
        public string FailoverKey { get; set; }
        public TimeSpan? FailoverRetryLatency { get; set; }
    }
}
