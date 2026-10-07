using System.Collections;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;

namespace OrchardCore.Workflows.Expressions;

/// <summary>
/// The text of the expression is its value, converted to the expected type: text as is, booleans and numbers in
/// the invariant culture, JSON for objects, and JSON arrays or comma-separated text for lists.
/// </summary>
public sealed class LiteralExpressionProvider : IWorkflowExpressionProvider
{
    internal readonly IStringLocalizer S;

    public LiteralExpressionProvider(IStringLocalizer<LiteralExpressionProvider> localizer)
    {
        S = localizer;
    }

    public string Name => WorkflowExpressionSyntaxes.Literal;

    public LocalizedString DisplayName => S["Literal"];

    public string EditorLanguage => "plaintext";

    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context)
    {
        if (!TryConvert(expression?.Expression, typeof(T), out var value))
        {
            throw new FormatException($"The literal '{expression?.Expression}' isn't a valid {Describe(typeof(T))}.");
        }

        return Task.FromResult((T)value);
    }

    public IReadOnlyList<string> Validate(string expression, Type valueType)
        => TryConvert(expression, valueType, out _) ? [] : [S["'{0}' isn't a valid {1}.", expression, Describe(valueType)]];

    /// <summary>
    /// Converts the text of a literal to <paramref name="type"/>.
    /// </summary>
    public static bool TryConvert(string text, Type type, out object value)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;

        if (target == typeof(string) || target == typeof(object))
        {
            value = text;
            return true;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            value = type.IsValueType && Nullable.GetUnderlyingType(type) is null ? Activator.CreateInstance(type) : null;
            return true;
        }

        var trimmed = text.Trim();

        if (target == typeof(bool))
        {
            var parsed = bool.TryParse(trimmed, out var flag);
            value = flag;
            return parsed;
        }

        if (target.IsPrimitive || target == typeof(decimal))
        {
            try
            {
                value = Convert.ChangeType(trimmed, target, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception exception) when (exception is FormatException or OverflowException or InvalidCastException)
            {
                value = null;
                return false;
            }
        }

        if (target.IsAssignableFrom(typeof(List<object>)))
        {
            return TryReadList(trimmed, out value);
        }

        try
        {
            value = JsonSerializer.Deserialize(trimmed, target, JOptions.Default);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    // A JSON array, or comma-separated values.
    private static bool TryReadList(string text, out object value)
    {
        if (text.StartsWith('['))
        {
            try
            {
                value = JsonNode.Parse(text) is JsonArray array ? (List<object>)VariableValues.FromJson(array) : null;
                return value is not null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        value = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Cast<object>().ToList();
        return true;
    }

    private string Describe(Type type)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;

        if (target == typeof(bool))
        {
            return S["true or false value"];
        }

        if (target.IsPrimitive || target == typeof(decimal))
        {
            return S["number"];
        }

        return typeof(IEnumerable).IsAssignableFrom(target) ? S["list"] : S["JSON value"];
    }
}
