using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.ContentManagement.Metadata.Documents
{
    public class ContentDefinitionDocument
    {
        public ContentDefinitionDocument()
        {
            ContentTypeDefinitions = new List<ContentTypeDefinitionDocument>();
            ContentPartDefinitions = new List<ContentPartDefinitionDocument>();
        }

        public IList<ContentTypeDefinitionDocument> ContentTypeDefinitions { get; set; }
        public IList<ContentPartDefinitionDocument> ContentPartDefinitions { get; set; }

        public int Serial { get; set; }

        [Obsolete("For compatibility with previous naming convention")]
        [JsonProperty("ContentTypeDefinitions")]
        private IList<ContentTypeDefinitionDocument> ContentTypeDefinitionDocuments { set { ContentTypeDefinitions = value; } }

        [Obsolete("For compatibility with previous naming convention")]
        [JsonProperty("ContentPartDefinitions")]
        private IList<ContentPartDefinitionDocument> ContentPartDefinitionDocuments { set { ContentPartDefinitions = value; } }
    }
}