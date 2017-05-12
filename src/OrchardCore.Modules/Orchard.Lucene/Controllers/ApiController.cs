using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace Orchard.Lucene.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost, HttpGet]
        public async Task<IActionResult> Content(
            string indexName,
            string query,
            string parameters,
            [FromServices] LuceneQuerySource luceneQuerySource,
            [FromServices] ISession session)
        {
            var luceneQuery = new LuceneQuery
            {
                Index = indexName,
                Template = query
            };

            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters ?? "");

            var result = await luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);

            // Load corresponding content item versions
            var contentItemVersionIds = (result as JArray).Select(x => ((JObject)x)["Content.ContentItem.ContentItemVersionId"].Value<string>()).ToArray();
            var contentItems = await session.QueryAsync<ContentItem, ContentItemIndex>(x => x.ContentItemVersionId.IsIn(contentItemVersionIds)).List();

            return new ObjectResult(contentItems);
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> Documents(
            string indexName,
            string query,
            string parameters,
            [FromServices] LuceneQuerySource luceneQuerySource,
            [FromServices] ISession session)
        {
            var luceneQuery = new LuceneQuery
            {
                Index = indexName,
                Template = query
            };

            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters ?? "");

            var result = await luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);

            return new ObjectResult(result);
        }
    }
}
