using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Controllers
{
    [Route("api/lucene")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class LuceneApiController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IQuerySource _querySource;
        private readonly LuceneQuerySource _luceneQuerySource;

        public LuceneApiController(
            IAuthorizationService authorizationService,
            [FromKeyedServices(LuceneQuerySource.SourceName)] IQuerySource querySource,
            LuceneQuerySource luceneQuerySource)
        {
            _authorizationService = authorizationService;
            _querySource = querySource;
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

        private Task<IQueryResults> LuceneQueryApiAsync(LuceneQueryModel queryModel, bool returnContentItems = false)
        {
            var luceneQuery = _querySource.Create();
            luceneQuery.ReturnContentItems = returnContentItems;

            luceneQuery.Put(new LuceneQueryMetadata()
            {
                Index = queryModel.IndexName,
                Template = queryModel.Query,
            });

            var queryParameters = queryModel.Parameters != null ?
                JConvert.DeserializeObject<Dictionary<string, object>>(queryModel.Parameters)
                : [];

            return _luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);
        }
    }
}
