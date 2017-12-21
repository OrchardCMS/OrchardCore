using System.Collections.Generic;
using System.Linq;
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
            return (await razorHelper.QueryAsync(queryName, parameters)).Select(o => ((JObject)o).ToObject<ContentItem>());
        }
    }
}