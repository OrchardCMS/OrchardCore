using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Lucene.Controllers
{
    [Route("api/lucene")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly LuceneQuerySource _luceneQuerySource;

        public ApiController(
            IAuthorizationService authorizationService,
            LuceneQuerySource luceneQuerySource)
        {
            _authorizationService = authorizationService;
            _luceneQuerySource = luceneQuerySource;
        }

        [HttpGet]
        [Route("content")]
        public async Task<IActionResult> Content(
            string indexName,
            string query,
            string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(indexName, query, parameters, returnContentItems: true);

            return new ObjectResult(result);
        }
        
        [HttpPost]
        [Route("content")]
        public async Task<IActionResult> ContentPost(
            [FromForm] string indexName,
            [FromForm] string query,
            [FromForm] string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(indexName, query, parameters, returnContentItems: true);

            return new ObjectResult(result);
        }

        [HttpGet]
        [Route("documents")]
        public async Task<IActionResult> Documents(
            string indexName,
            string query,
            string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(indexName, query, parameters);

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("documents")]
        public async Task<IActionResult> DocumentsPost(
            [FromForm] string indexName,
            [FromForm] string query,
            [FromForm] string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(indexName, query, parameters);

            return new ObjectResult(result);
        }

        private async Task<Queries.IQueryResults> LuceneQueryApiAsync(string indexName, string query, string parameters, bool returnContentItems = false)
        {
            var luceneQuery = new LuceneQuery
            {
                Index = indexName,
                Template = query,
                ReturnContentItems = returnContentItems
            };

            var queryParameters = parameters != null ?
                JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                : new Dictionary<string, object>();

            var result = await _luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);
            return result;
        }
    }
}
