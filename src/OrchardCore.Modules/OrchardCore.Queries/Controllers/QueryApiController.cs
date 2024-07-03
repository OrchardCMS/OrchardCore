using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Queries.Indexes;
using YesSql;

namespace OrchardCore.Queries.Controllers
{
    [Route("api/queries")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class QueryApiController : ControllerBase
    {
        private readonly YesSql.ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthorizationService _authorizationService;

        public QueryApiController(
            IAuthorizationService authorizationService,
            YesSql.ISession session,
            IServiceProvider serviceProvider
            )
        {
            _authorizationService = authorizationService;
            _session = session;
            _serviceProvider = serviceProvider;
        }

        [HttpPost, HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> Query(
            string name,
            string parameters)
        {
            var query = await _session.Query<Query, QueryIndex>(q => q.Name == name).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.CreatePermissionForQuery(query.Name)))
            {
                // Intentionally not returning Unauthorized as it is not usable from APIs and would
                // expose the existence of a named query (not a concern per se).
                return NotFound();
            }

            if (Request.Method == HttpMethods.Post && string.IsNullOrEmpty(parameters))
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                parameters = await reader.ReadToEndAsync();
            }

            var queryParameters = !string.IsNullOrEmpty(parameters) ?
                JConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                : [];

            var querySource = _serviceProvider.GetRequiredKeyedService<IQuerySource>(query.Source);

            var result = await querySource.ExecuteQueryAsync(query, queryParameters);

            return new ObjectResult(result);
        }
    }
}
