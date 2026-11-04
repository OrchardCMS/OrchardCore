using System.Text.Json.Nodes;
using OrchardCore.Queries.Endpoints.Api;

namespace OrchardCore.Queries.Services;

internal interface IQuerySourceApiDescriptorProvider
{
    string Source { get; }

    string DisplayName { get; }

    string Description { get; }

    JsonNode BuildPropertiesSchema();

    ValueTask ValidateAsync(QueryDefinitionDto definition, QueryValidationResultDto result);
}
