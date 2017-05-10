using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Lucene.ViewModels;
using Orchard.Queries;
using YesSql;
using YesSql.Services;

namespace Orchard.Lucene.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost, HttpGet]
        public async Task<IActionResult> Content(
            ApiQueryViewModel model, 
            [FromServices] IQueryManager queryManager,
            [FromServices] ISession session)
        {
            var luceneQuery = new LuceneQuery
            {
                Index = model.IndexName,
                Template = model.Query
            };

            var parameters = String.IsNullOrEmpty(model.Parameters) ? new JObject() : JObject.Parse(model.Parameters);

            var result = await queryManager.ExecuteQueryAsync(luceneQuery, parameters);

            // Load corresponding content items
            var contentItemIds = (result as JArray).Select(x => ((JObject)x)["ContentItemId"].Value<string>()).ToArray();
            var contentItems = await session.QueryAsync<ContentItem, ContentItemIndex>(x => x.Published && x.ContentItemId.IsIn(contentItemIds)).List();

            return new ObjectResult(contentItems);
        }
    }
}
