using System;

namespace OrchardCore.Documents.Options
{
    public interface IDocumentSharedOptions
    {
        TimeSpan? FailoverRetryLatency { get; set; }
    }

    public class DocumentSharedOptions : DocumentOptionsBase, IDocumentSharedOptions
    {
        public TimeSpan? FailoverRetryLatency { get; set; }
    }
}
