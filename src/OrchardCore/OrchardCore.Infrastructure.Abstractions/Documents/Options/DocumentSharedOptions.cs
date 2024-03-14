using System;

namespace OrchardCore.Documents.Options
{
    public class DocumentSharedOptions : DocumentOptionsBase, IDocumentSharedOptions
    {
        public TimeSpan? FailoverRetryLatency { get; set; }
    }
}
