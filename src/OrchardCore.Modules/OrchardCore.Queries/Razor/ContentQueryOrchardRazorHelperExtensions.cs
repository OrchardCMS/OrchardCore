using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Queries;

namespace OrchardCore.ContentManagement
{
    public static class ContentQueryOrchardRazorHelperExtensions
    {
        public static async Task<IEnumerable<ContentItem>> ContentQueryAsync(this OrchardRazorHelper razorHelper, string queryName)
        {
            return await ContentQueryAsync(razorHelper, queryName, new Dictionary<string, object>());
        }

        public static async Task<IEnumerable<ContentItem>> ContentQueryAsync(this OrchardRazorHelper razorHelper, string queryName, IDictionary<string, object> parameters)
        {
            var results = await razorHelper.QueryAsync(queryName, parameters);
            var contentItems = new List<ContentItem>();

            foreach (var result in results)
            {
                if (!(result is ContentItem contentItem))
                {
                    contentItem = null;

                    if (result is JObject jObject)
                    {
                        contentItem = jObject.ToObject<ContentItem>();
                    }
                }

                // If input is a 'JObject' but which not represents a 'ContentItem',
                // a 'ContentItem' is still created but with some null properties.
                if (contentItem?.ContentItemId == null)
                {
                    continue;
                }

                contentItems.Add(contentItem);
            }

            return contentItems;
        }
    }
}