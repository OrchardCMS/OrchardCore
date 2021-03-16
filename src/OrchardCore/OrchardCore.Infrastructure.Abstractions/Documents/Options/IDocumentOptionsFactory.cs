using System;

namespace OrchardCore.Documents.Options
{
    public interface IDocumentOptionsFactory
    {
        DocumentOptions Create(Type documentType);
    }
}
