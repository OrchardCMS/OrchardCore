using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentManagement.Metadata.Settings;

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

            //TODO all testing code. use Builder
            var typeSettings = new ContentTypeSettings()
            {
                Creatable = true,
                Draftable = true,
                Securable = true,
                Listable = true,
                Versionable = true
            };

            var partSettings = new ContentTypePartSettings()
            {
                Position = "0"
            };


            ContentTypeDefinitionRecord = new ContentTypeDefinitionRecord();
            ContentTypeDefinitionRecord.Name = "TestCodeContentType";
            ContentTypeDefinitionRecord.DisplayName = "Test Code Content Type";
            ContentTypeDefinitionRecord.Settings = new JObject();
            ContentTypeDefinitionRecord.Settings[typeof(ContentTypeSettings).Name] = JObject.FromObject(typeSettings);

            var testContentPartA = new ContentTypePartDefinitionRecord()
            {
                PartName = "TestContentPartA",
                Name = "TestContentPartA"
            };
            testContentPartA.Settings = new JObject();
            testContentPartA.Settings[typeof(ContentTypeSettings).Name] = JObject.FromObject(partSettings);


            ContentTypeDefinitionRecord.ContentTypePartDefinitionRecords = new List<ContentTypePartDefinitionRecord>()
            {
               testContentPartA
            };

            // TODO : Fields. 
        }

        public Type Type { get; }

        public ContentTypeDefinitionRecord ContentTypeDefinitionRecord { get; }
    }
}
