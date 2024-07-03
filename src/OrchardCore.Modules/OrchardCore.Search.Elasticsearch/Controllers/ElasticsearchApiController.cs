using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch
{
    [Route("api/elasticsearch")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ElasticsearchApiController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IQuerySource _querySource;
        private readonly ElasticQuerySource _elasticQuerySource;

        public ElasticsearchApiController(
            IAuthorizationService authorizationService,
            [FromKeyedServices(ElasticQuerySource.SourceName)] IQuerySource querySource,
            ElasticQuerySource elasticQuerySource)
        {
            _authorizationService = authorizationService;
            _querySource = querySource;
            _elasticQuerySource = elasticQuerySource;
        }

        [HttpGet]
        [Route("content")]
        public async Task<IActionResult> Content([FromQuery] ElasticApiQueryViewModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            var result = await ElasticQueryApiAsync(queryModel, returnContentItems: true);

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("content")]
        public async Task<IActionResult> ContentPost(ElasticApiQueryViewModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await ElasticQueryApiAsync(queryModel, returnContentItems: true);

            return new ObjectResult(result);
        }

        [HttpGet]
        [Route("documents")]
        public async Task<IActionResult> Documents([FromQuery] ElasticApiQueryViewModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await ElasticQueryApiAsync(queryModel);

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("documents")]
        public async Task<IActionResult> DocumentsPost(ElasticApiQueryViewModel queryModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            var result = await ElasticQueryApiAsync(queryModel);

            return new ObjectResult(result);
        }

        private Task<IQueryResults> ElasticQueryApiAsync(ElasticApiQueryViewModel queryModel, bool returnContentItems = false)
        {
            var elasticQuery = _querySource.Create();
            elasticQuery.ReturnContentItems = returnContentItems;

            elasticQuery.Put(new ElasticsearchQueryMetadata
            {
                Index = queryModel.IndexName,
                Template = queryModel.Query,
            });

            var queryParameters = queryModel.Parameters != null ?
                JConvert.DeserializeObject<Dictionary<string, object>>(queryModel.Parameters)
                : [];

            var result = _elasticQuerySource.ExecuteQueryAsync(elasticQuery, queryParameters);

            return result;
        }
    }
}
