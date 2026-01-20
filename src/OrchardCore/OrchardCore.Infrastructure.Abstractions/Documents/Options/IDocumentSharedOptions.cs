using System;

namespace OrchardCore.Documents.Options;

public interface IDocumentSharedOptions
{
    TimeSpan? FailoverRetryLatency { get; set; }
}
