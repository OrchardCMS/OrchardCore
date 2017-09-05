using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using YesSql;

namespace OrchardCore.Lucene.Controllers
{
    public class ApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly LuceneQuerySource _luceneQuerySource;
        private readonly ISession _session;

        public ApiController(
            IAuthorizationService authorizationService,
            LuceneQuerySource luceneQuerySource,
            ISession session)
        {
            _authorizationService = authorizationService;
            _luceneQuerySource = luceneQuerySource;
            _session = session;
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> Content(
            string indexName,
            string query,
            string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return Unauthorized();
            }

            var luceneQuery = new LuceneQuery
            {
                Index = indexName,
                Template = query,
                ReturnContentItems = true
            };

            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters ?? "");

            var result = await _luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);

            return new ObjectResult(result);
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> Documents(
            string indexName,
            string query,
            string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return Unauthorized();
            }

            var luceneQuery = new LuceneQuery
            {
                Index = indexName,
                Template = query
            };

            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters ?? "");

            var result = await _luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);

            return new ObjectResult(result);
        }
    }
}
