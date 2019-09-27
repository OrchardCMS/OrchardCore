using System;
using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentManagement
{
    public class CodeContentTypeOption
    {
        public CodeContentTypeOption(Type contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            Type = contentType;
            ContentTypeDefinitionRecord = new ContentTypeDefinitionRecord();
            ContentTypeDefinitionRecord.Name = "TestContentType";
            ContentTypeDefinitionRecord.DisplayName = "Test Content Type";
        }

        public Type Type { get; }

        public ContentTypeDefinitionRecord ContentTypeDefinitionRecord { get; }
    }
}
