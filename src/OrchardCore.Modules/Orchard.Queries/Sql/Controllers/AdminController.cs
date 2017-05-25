using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Orchard.Queries.Sql.ViewModels;
using Orchard.Tokens.Services;
using YesSql;

namespace Orchard.Queries.Sql.Controllers
{
    [Feature("Orchard.Queries.Sql")]
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStore _store;
        private readonly ITokenizer _tokenizer;
        private readonly IStringLocalizer<AdminController> _stringLocalizer;

        public AdminController(
            IAuthorizationService authorizationService, 
            IStore store, 
            ITokenizer tokenizer,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _store = store;
            _tokenizer = tokenizer;
            _stringLocalizer = stringLocalizer;
        }

        public Task<IActionResult> Query(string query, [FromServices] ITokenizer tokenizer)
        {
            query = String.IsNullOrWhiteSpace(query) ? "" : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(query));
            return Query(new AdminQueryViewModel { DecodedQuery = query }, tokenizer);
        }

        [HttpPost]
        public async Task<IActionResult> Query(AdminQueryViewModel model, [FromServices] ITokenizer tokenizer)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSqlQueries))
            {
                return Unauthorized();
            }

            if (String.IsNullOrWhiteSpace(model.DecodedQuery))
            {
                return View(model);
            }

            if (String.IsNullOrEmpty(model.Parameters))
            {
                model.Parameters = "{ }";
            }
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            var dialect = SqlDialectFactory.For(connection);

            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);
            var tokenizedQuery = _tokenizer.Tokenize(model.DecodedQuery, parameters);

            if (SqlParser.TryParse(tokenizedQuery, dialect, _store.Configuration.TablePrefix, out var rawQuery, out var rawParameters, out var messages))
            {
                model.RawSql = rawQuery;
                model.RawParameters = JsonConvert.SerializeObject(rawParameters);

                try
                {
                    using (connection)
                    {
                        connection.Open();
                        model.Documents = await connection.QueryAsync(rawQuery, rawParameters);
                    }
                }
                catch(Exception e)
                {
                    ModelState.AddModelError("", _stringLocalizer["An error occured while executing the SQL query: {0}", e.Message]);
                }
            }
            else
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError("", message);
                }
            }

            model.Elapsed = stopwatch.Elapsed;

            return View(model);
        }
    }
}
