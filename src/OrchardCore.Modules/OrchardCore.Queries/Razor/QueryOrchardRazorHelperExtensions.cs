using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Razor;

namespace OrchardCore.Queries
{
    public static class QueryOrchardRazorHelperExtensions
    {
        public static async Task<IEnumerable<object>> QueryAsync(this OrchardRazorHelper razorHelper, string queryName)
        {
            return await QueryAsync(razorHelper, queryName, new Dictionary<string, object>());
        }

        public static async Task<IEnumerable<object>> QueryAsync(this OrchardRazorHelper razorHelper, string queryName, IDictionary<string, object> parameters)
        {
            var queryManager = razorHelper.HttpContext.RequestServices.GetService<IQueryManager>();

            var query = await queryManager.GetQueryAsync(queryName);

            if (query == null)
            {
                return null;
            }

            return (IEnumerable<object>)await queryManager.ExecuteQueryAsync(query, parameters);
        }
    }
}
