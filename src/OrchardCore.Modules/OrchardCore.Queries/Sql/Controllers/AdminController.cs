using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Queries.Sql.ViewModels;
using YesSql;

namespace OrchardCore.Queries.Sql.Controllers
{
    [Feature("OrchardCore.Queries.Sql")]
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStore _store;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IStringLocalizer<AdminController> _stringLocalizer;

        public AdminController(
            IAuthorizationService authorizationService,
            IStore store,
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<AdminController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _store = store;
            _liquidTemplateManager = liquidTemplateManager;
            _stringLocalizer = stringLocalizer;
        }

        public Task<IActionResult> Query(string query)
        {
            query = String.IsNullOrWhiteSpace(query) ? "" : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(query));
            return Query(new AdminQueryViewModel
            {
                DecodedQuery = query,
                FactoryName = _store.Configuration.ConnectionFactory.GetType().FullName
            });
        }

        [HttpPost]
        public async Task<IActionResult> Query(AdminQueryViewModel model)
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

            var templateContext = new TemplateContext();
            foreach(var parameter in parameters)
            {
                templateContext.SetValue(parameter.Key, parameter.Value);
            }

            var tokenizedQuery = await _liquidTemplateManager.RenderAsync(model.DecodedQuery, NullEncoder.Default, templateContext);

            model.FactoryName = _store.Configuration.ConnectionFactory.GetType().FullName;

            if (SqlParser.TryParse(tokenizedQuery, dialect, _store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
            {
                model.RawSql = rawQuery;
                model.Parameters = JsonConvert.SerializeObject(parameters, Formatting.Indented);

                try
                {
                    using (connection)
                    {
                        connection.Open();
                        model.Documents = await connection.QueryAsync(rawQuery, parameters);
                    }
                }
                catch(Exception e)
                {
                    ModelState.AddModelError("", _stringLocalizer["An error occurred while executing the SQL query: {0}", e.Message]);
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
