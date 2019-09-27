using OrchardCore.ContentManagement.Metadata.Records;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    public interface ICodeContentDefinitionService
    {
        IList<ContentTypeDefinitionRecord> GetContentTypeDefinitions();
    }
}