using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Variables;

/// <summary>
/// Any value, unchanged. JSON values become their CLR form.
/// </summary>
public sealed class AnyVariableType : IWorkflowVariableType
{
    internal readonly IStringLocalizer S;

    public AnyVariableType(IStringLocalizer<AnyVariableType> localizer)
    {
        S = localizer;
    }

    public string Name => "any";

    public LocalizedString DisplayName => S["Any"];

    public string Editor => "json";

    public bool TryCoerce(object value, out object result)
    {
        result = value is JsonNode node ? VariableValues.FromJson(node) : value;

        return true;
    }
}
