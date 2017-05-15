using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Orchard.Queries.Controllers
{
    public class ApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IQueryManager _queryManager;

        public ApiController(
            IAuthorizationService authorizationService,
            IQueryManager queryManager
            )
        {
            _authorizationService = authorizationService;
            _queryManager = queryManager;
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> Query(
            string name,
            string parameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ExecuteQueryApi))
            {
                return Unauthorized();
            }

            var queryParameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters ?? "");
            var query = await _queryManager.GetQueryAsync(name);

            if (query == null)
            {
                return NotFound();
            }

            var result = await _queryManager.ExecuteQueryAsync(query, queryParameters);

            return new ObjectResult(result);
        }
    }
}
