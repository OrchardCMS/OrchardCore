using System.Collections.Generic;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class FieldMetaDataExtensions
    {
        public static IDictionary<string, object> AddPartMetaData(this IDictionary<string, object> metaData, string partName, bool partCollapsed = false)
        {
            metaData = metaData ?? new Dictionary<string, object>();

            if (!metaData.ContainsKey("PartCollapsed"))
            {
                metaData.Add("PartCollapsed", partCollapsed);
            }

            if (!metaData.ContainsKey("PartName"))
            {
                metaData.Add("PartName", partName);
            }

            return metaData;
        }
    }
}
