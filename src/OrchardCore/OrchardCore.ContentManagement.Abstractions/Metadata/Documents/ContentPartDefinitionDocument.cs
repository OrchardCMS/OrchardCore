using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Documents
{
    public class ContentPartDefinitionDocument
    {
        public ContentPartDefinitionDocument()
        {
            ContentPartFieldDefinitions = new List<ContentPartFieldDefinitionDocument>();
            Settings = new JObject();
        }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of a part, like description, or any property that a module would attach
        /// to a part.
        /// </summary>
        public JObject Settings { get; set; }

        public IList<ContentPartFieldDefinitionDocument> ContentPartFieldDefinitions { get; set; }

        [Obsolete("For compatibility with previous naming convention")]
        [JsonProperty("ContentPartFieldDefinitionRecords")]
        private IList<ContentPartFieldDefinitionDocument> ContentPartFieldDefinitionRecords { set { ContentPartFieldDefinitions = value; } }
    }
}