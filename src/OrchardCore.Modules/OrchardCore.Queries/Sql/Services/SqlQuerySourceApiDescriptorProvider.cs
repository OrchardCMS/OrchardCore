using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Liquid;
using OrchardCore.Queries.Endpoints.Api;
using OrchardCore.Queries.Services;
using OrchardCore.Queries.Sql.Models;
using YesSql;

namespace OrchardCore.Queries.Sql.Services;

internal sealed class SqlQuerySourceApiDescriptorProvider : IQuerySourceApiDescriptorProvider
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly SqlLiquidOutputExpressionDetector _outputExpressionDetector;
    private readonly IStore _store;
    private readonly IStringLocalizer<SqlQuerySourceApiDescriptorProvider> _localizer;

    public SqlQuerySourceApiDescriptorProvider(
        ILiquidTemplateManager liquidTemplateManager,
        SqlLiquidOutputExpressionDetector outputExpressionDetector,
        IStore store,
        IStringLocalizer<SqlQuerySourceApiDescriptorProvider> stringLocalizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _outputExpressionDetector = outputExpressionDetector;
        _store = store;
        _localizer = stringLocalizer;
    }

    public string Source => SqlQuerySource.SourceName;

    public string DisplayName => "SQL";

    public string Description => "Runs a SQL template after Liquid rendering against the current tenant database.";

    public JsonNode BuildPropertiesSchema() => new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            [nameof(SqlQueryMetadata)] = new JsonObject
            {
                ["type"] = "object",
                ["required"] = new JsonArray(nameof(SqlQueryMetadata.Template)),
                ["properties"] = new JsonObject
                {
                    [nameof(SqlQueryMetadata.Template)] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The SQL template to render and execute.",
                    },
                },
            },
        },
    };

    public ValueTask ValidateAsync(QueryDefinitionDto definition, QueryValidationResultDto result)
    {
        if (definition.Properties?.TryGetPropertyValue(nameof(SqlQueryMetadata), out var metadataNode) != true || metadataNode is not JsonObject metadata)
        {
            result.Errors.Add(_localizer["The SQL query metadata is required."].Value);
            return ValueTask.CompletedTask;
        }

        var template = metadata[nameof(SqlQueryMetadata.Template)]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(template))
        {
            result.Errors.Add(_localizer["The SQL template is required."].Value);
            return ValueTask.CompletedTask;
        }

        if (!_liquidTemplateManager.Validate(template, out var liquidErrors))
        {
            foreach (var error in liquidErrors)
            {
                result.Errors.Add(error);
            }

            return ValueTask.CompletedTask;
        }

        if (_outputExpressionDetector.ContainsOutputStatement(template))
        {
            result.Warnings.Add(_localizer["Potentially unsafe Liquid output expressions ('{{ ... }}') were detected. Prefer SQL parameters when injecting user input."].Value);
        }

        var parameters = new Dictionary<string, object>();

        if (!SqlParser.TryParse(template, _store.Configuration.Schema, _store.Configuration.SqlDialect, _store.Configuration.TablePrefix, parameters, out _, out var messages))
        {
            foreach (var message in messages)
            {
                result.Errors.Add(message);
            }
        }

        return ValueTask.CompletedTask;
    }
}
