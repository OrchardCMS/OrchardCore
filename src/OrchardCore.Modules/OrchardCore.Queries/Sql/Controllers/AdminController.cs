using System.Diagnostics;
using System.Text.Json;
using Dapper;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Queries.Sql.ViewModels;
using YesSql;

namespace OrchardCore.Queries.Sql.Controllers;

[Feature("OrchardCore.Queries.Sql")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IStore _store;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly TemplateOptions _templateOptions;

    internal readonly IStringLocalizer S;

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

    [Admin("Queries/Sql/Query", "QueriesRunSql")]
    public Task<IActionResult> Query(string query)
    {
        query = string.IsNullOrWhiteSpace(query)
            ? ""
            : Base64.FromUTF8Base64String(query);

        return Query(new AdminQueryViewModel
        {
            DecodedQuery = query,
            FactoryName = _store.Configuration.ConnectionFactory.GetType().FullName
        });
    }

    [HttpPost]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        model.FactoryName = _store.Configuration.ConnectionFactory.GetType().FullName;

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSqlQueries))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(model.DecodedQuery))
        {
            return View(model);
        }

        if (string.IsNullOrEmpty(model.Parameters))
        {
            model.Parameters = "{ }";
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var dialect = _store.Configuration.SqlDialect;

        var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

        var tokenizedQuery = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, NullEncoder.Default, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions))));

        if (SqlParser.TryParse(tokenizedQuery, _store.Configuration.Schema, dialect, _store.Configuration.TablePrefix, parameters, out var rawQuery, out var messages))
        {
            model.RawSql = rawQuery;
            model.Parameters = JConvert.SerializeObject(parameters, JOptions.Indented);

            try
            {
                await using var connection = _store.Configuration.ConnectionFactory.CreateConnection();
                await connection.OpenAsync();
                model.Documents = await connection.QueryAsync(rawQuery, parameters);
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
