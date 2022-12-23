using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Controllers
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
        public async Task<IActionResult> Content([FromQuery] LuceneQueryModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            var result = await LuceneQueryApiAsync(queryModel, returnContentItems: true);

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("content")]
        public async Task<IActionResult> ContentPost(LuceneQueryModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(queryModel, returnContentItems: true);

            return new ObjectResult(result);
        }

        [HttpGet]
        [Route("documents")]
        public async Task<IActionResult> Documents([FromQuery] LuceneQueryModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(queryModel);

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("documents")]
        public async Task<IActionResult> DocumentsPost(LuceneQueryModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            var result = await LuceneQueryApiAsync(queryModel);

            return new ObjectResult(result);
        }

        private Task<Queries.IQueryResults> LuceneQueryApiAsync(LuceneQueryModel queryModel, bool returnContentItems = false)
        {
            var luceneQuery = new LuceneQuery
            {
                Index = queryModel.IndexName,
                Template = queryModel.Query,
                ReturnContentItems = returnContentItems
            };

            var queryParameters = queryModel.Parameters != null ?
                JsonConvert.DeserializeObject<Dictionary<string, object>>(queryModel.Parameters)
                : new Dictionary<string, object>();

            var result = _luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);
            return result;
        }
    }
}
