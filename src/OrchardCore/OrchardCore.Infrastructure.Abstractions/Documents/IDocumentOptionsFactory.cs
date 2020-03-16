using System;

namespace OrchardCore.Documents
{
    public interface IDocumentOptionsFactory
    {
        DocumentOptions Create(Type documentType);
    }
}
