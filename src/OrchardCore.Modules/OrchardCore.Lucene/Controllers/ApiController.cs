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
        public async Task<IActionResult> Content([FromQuery] LuceneQueryDTO queryDto)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(queryDto, returnContentItems: true);

            return new ObjectResult(result);
        }
        
        [HttpPost]
        [Route("content")]
        public async Task<IActionResult> ContentPost(LuceneQueryDTO queryDto)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(queryDto, returnContentItems: true);

            return new ObjectResult(result);
        }

        [HttpGet]
        [Route("documents")]
        public async Task<IActionResult> Documents([FromQuery] LuceneQueryDTO queryDto)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            } 

            var result = await LuceneQueryApiAsync(queryDto);

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route("documents")]
        public async Task<IActionResult> DocumentsPost(LuceneQueryDTO queryDto)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
            {
                return this.ChallengeOrForbid();
            }

            var result = await LuceneQueryApiAsync(queryDto);

            return new ObjectResult(result);
        }

        private Task<Queries.IQueryResults> LuceneQueryApiAsync(LuceneQueryDTO queryDto, bool returnContentItems = false)
        {
            var luceneQuery = new LuceneQuery
            {
                Index = queryDto.IndexName,
                Template = queryDto.Query,
                ReturnContentItems = returnContentItems
            };

            var queryParameters = queryDto.Parameters != null ?
                JsonConvert.DeserializeObject<Dictionary<string, object>>(queryDto.Parameters)
                : new Dictionary<string, object>();

            var result = _luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);
            return result;
        }
    }
}
