using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
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
        protected readonly IStringLocalizer S;
        private readonly TemplateOptions _templateOptions;

        public AdminController(
            IAuthorizationService authorizationService,
            IStore store,
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<AdminController> stringLocalizer,
            IOptions<TemplateOptions> templateOptions)

        {
            _authorizationService = authorizationService;
            _store = store;
            _liquidTemplateManager = liquidTemplateManager;
            S = stringLocalizer;
            _templateOptions = templateOptions.Value;
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
                return Forbid();
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
            var dialect = _store.Configuration.SqlDialect;

            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

            var tokenizedQuery = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, NullEncoder.Default, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

            model.FactoryName = _store.Configuration.ConnectionFactory.GetType().FullName;

            if (SqlParser.TryParse(tokenizedQuery, _store.Configuration.Schema, dialect, _store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
            {
                model.RawSql = rawQuery;
                model.Parameters = JsonConvert.SerializeObject(parameters, Formatting.Indented);

                try
                {
                    using (connection)
                    {
                        await connection.OpenAsync();
                        model.Documents = await connection.QueryAsync(rawQuery, parameters);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", S["An error occurred while executing the SQL query: {0}", e.Message]);
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
