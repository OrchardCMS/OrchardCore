using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Documents
{
    public class ContentTypeDefinitionDocument
    {
        public ContentTypeDefinitionDocument()
        {
            ContentTypePartDefinitions = new List<ContentTypePartDefinitionDocument>();
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public JObject Settings { get; set; }

        public IList<ContentTypePartDefinitionDocument> ContentTypePartDefinitions { get; set; }


        [Obsolete("For compatibility with previous naming convention")]
        [JsonProperty("ContentTypePartDefinitionRecords")]
        private IList<ContentTypePartDefinitionDocument> ContentTypePartDefinitionRecords { set { ContentTypePartDefinitions = value; } }
    }
}