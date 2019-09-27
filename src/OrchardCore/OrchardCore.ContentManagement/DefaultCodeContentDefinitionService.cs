using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Records;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement
{
    public class DefaultCodeContentDefinitionService : ICodeContentDefinitionService
    {
        private readonly ContentOptions _contentOptions;

        public DefaultCodeContentDefinitionService(
            IOptions<ContentOptions> contentOptions
        )
        {
            _contentOptions = contentOptions.Value;
        }

        public IList<ContentTypeDefinitionRecord> GetContentTypeDefinitions()
        {
            var typeRecords = new List<ContentTypeDefinitionRecord>();
            foreach(var contentTypeOption in _contentOptions.CodeContentTypeOptions){
                typeRecords.Add(contentTypeOption.ContentTypeDefinitionRecord);
            }
            return typeRecords;
        }
    }
}