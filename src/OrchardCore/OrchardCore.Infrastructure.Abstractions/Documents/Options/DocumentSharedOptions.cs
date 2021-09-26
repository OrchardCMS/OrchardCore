using System;

namespace OrchardCore.Documents.Options
{
    public interface IDocumentSharedOptions
    {
        string FailoverKey { get; set; }
        TimeSpan? FailoverRetryLatency { get; set; }
    }

    public class DocumentSharedOptions : DocumentOptionsBase, IDocumentSharedOptions
    {
        public string FailoverKey { get; set; }
        public TimeSpan? FailoverRetryLatency { get; set; }
    }
}
