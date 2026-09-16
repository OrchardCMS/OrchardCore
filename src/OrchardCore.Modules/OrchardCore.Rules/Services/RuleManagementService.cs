using System.Text.Json;
using System.Text.Json.Nodes;
using Jint;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services;

/// <summary>
/// Describes registered conditions and validates the built-in management contract.
/// Unknown extension conditions remain visible but cannot be overwritten silently.
/// </summary>
public sealed class RuleManagementService : IRuleManagementService
{
    private readonly IReadOnlyDictionary<string, IConditionFactory> _factories;
    private readonly ConditionOperatorOptions _operators;
    private readonly IConditionIdGenerator _ids;

    internal readonly IStringLocalizer S;

    public RuleManagementService(IEnumerable<IConditionFactory> factories,
        IOptions<ConditionOperatorOptions> operators, IConditionIdGenerator ids, IStringLocalizer<RuleManagementService> localizer)
    {
        _factories = factories.ToDictionary(factory => factory.Name, StringComparer.Ordinal);
        _operators = operators.Value;
        _ids = ids;
        S = localizer;
    }

    public IReadOnlyList<RuleConditionDescriptor> GetDescriptors() => _factories.Values
        .OrderBy(factory => factory.Name, StringComparer.Ordinal)
        .Select(factory =>
        {
            var condition = factory.Create();
            return new RuleConditionDescriptor
            {
                Name = factory.Name,
                CanWrite = IsSupported(condition),
                SupportsChildren = condition is ConditionGroup,
                PropertiesSchema = GetSchema(condition),
            };
        }).ToArray();

    public IReadOnlyList<RuleConditionDefinition> Describe(Rule rule) =>
        rule?.Conditions.Select(DescribeCondition).ToArray() ?? [];

    public IReadOnlyList<RuleConditionValidationFailure> ValidateCondition(Condition condition)
    {
        var errors = new List<RuleConditionValidationFailure>();
        if (condition is JavascriptCondition javascript)
        {
            if (string.IsNullOrWhiteSpace(javascript.Script))
            {
                errors.Add(new() { Property = "script", Message = S["Please provide a script."] });
            }
            else
            {
                try
                {
                    _ = Engine.PrepareScript(javascript.Script);
                }
                catch (ScriptPreparationException exception)
                {
                    errors.Add(new()
                    {
                        Property = "script",
                        Message = S["The script couldn't be parsed. Details: {0}", (exception.InnerException ?? exception).Message],
                    });
                }
            }
        }
        return errors;
    }

    public RuleManagementResult CreateRule(IReadOnlyList<RuleConditionDefinition> conditions, string ruleId = null)
    {
        var errors = new Dictionary<string, string[]>();
        var rule = new Rule { ConditionId = ruleId };
        if (string.IsNullOrEmpty(rule.ConditionId))
        {
            _ids.GenerateUniqueId(rule);
        }

        var identities = new HashSet<string>(StringComparer.Ordinal) { rule.ConditionId };
        var count = 0;
        ReadChildren(conditions, rule, "conditions", 0, identities, errors, ref count);
        return new RuleManagementResult { Rule = errors.Count == 0 ? rule : null, Errors = errors };
    }

    private void ReadChildren(IReadOnlyList<RuleConditionDefinition> definitions, ConditionGroup group,
        string path, int depth, HashSet<string> identities, Dictionary<string, string[]> errors, ref int count)
    {
        if (definitions is null || depth > 16 || definitions.Count > 256 || count + definitions.Count > 256)
        {
            errors[path] = ["Provide a conditions array with at most 256 total conditions and 16 nested groups."];
            return;
        }

        count += definitions.Count;
        for (var index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            var itemPath = $"{path}[{index}]";
            if (definition?.Name is null || !_factories.TryGetValue(definition.Name, out var factory)
                || !IsSupported(factory.Create()))
            {
                errors[itemPath + ".name"] = ["Choose a writable condition from the live condition descriptors."];
                continue;
            }

            var condition = factory.Create();
            var properties = definition.Properties;
            var schema = GetSchema(condition);
            if (properties is null || properties.Any(entry => !schema["properties"].AsObject().ContainsKey(entry.Key)))
            {
                errors[itemPath + ".properties"] = ["Provide an object containing only properties from the condition schema."];
                continue;
            }

            try
            {
                Populate(condition, properties);
            }
            catch (Exception exception) when (exception is InvalidOperationException or FormatException or JsonException)
            {
                errors[itemPath + ".properties"] = ["The condition properties do not match the live schema."];
                continue;
            }

            foreach (var error in ValidateCondition(condition))
            {
                errors[itemPath + ".properties." + error.Property] = [error.Message];
            }

            condition.ConditionId = definition.ConditionId;
            if (string.IsNullOrEmpty(condition.ConditionId))
            {
                _ids.GenerateUniqueId(condition);
            }

            if (condition.ConditionId.Length > 128 || condition.ConditionId.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_')
                || !identities.Add(condition.ConditionId))
            {
                errors[itemPath + ".conditionId"] = ["Condition IDs must be unique within the rule, at most 128 characters, and use letters, digits, hyphens or underscores."];
            }

            if (condition is ConditionGroup childGroup)
            {
                ReadChildren(definition.Conditions, childGroup, itemPath + ".conditions", depth + 1, identities, errors, ref count);
            }
            else if (definition.Conditions is null || definition.Conditions.Count != 0)
            {
                errors[itemPath + ".conditions"] = ["Only a group condition accepts children."];
            }

            group.Conditions.Add(condition);
        }
    }

    private void Populate(Condition condition, JsonObject properties)
    {
        switch (condition)
        {
            case BooleanCondition boolean:
                boolean.Value = properties["value"]?.GetValue<bool>() ?? throw new FormatException();
                break;
            case JavascriptCondition javascript:
                javascript.Script = properties["script"]?.GetValue<string>();
                break;
            case DisplayTextConditionGroup group:
                group.DisplayText = properties["displayText"]?.GetValue<string>();
                break;
            case UrlCondition url:
                (url.Value, url.Operation) = ReadStringOperation(properties);
                break;
            case CultureCondition culture:
                (culture.Value, culture.Operation) = ReadStringOperation(properties);
                break;
            case RoleCondition role:
                (role.Value, role.Operation) = ReadStringOperation(properties);
                break;
            case ContentTypeCondition contentType:
                (contentType.Value, contentType.Operation) = ReadStringOperation(properties);
                break;
        }
    }

    private (string, ConditionOperator) ReadStringOperation(JsonObject properties)
    {
        var value = properties["value"]?.GetValue<string>() ?? throw new FormatException();
        var name = properties["operation"]?.GetValue<string>();
        if (name is null || !_operators.Factories.TryGetValue(name, out var factory)
            || !IsSupportedOperator(factory.Create()))
        {
            throw new FormatException();
        }

        var operation = (StringOperator)factory.Create();
        operation.CaseSensitive = properties.ContainsKey("caseSensitive")
            ? properties["caseSensitive"]?.GetValue<bool>() ?? throw new FormatException() : false;
        return (value, operation);
    }

    private static bool IsSupported(Condition condition) => condition.GetType() == typeof(AllConditionGroup)
        || condition.GetType() == typeof(AnyConditionGroup) || condition.GetType() == typeof(BooleanCondition)
        || condition.GetType() == typeof(JavascriptCondition) || condition.GetType() == typeof(HomepageCondition)
        || condition.GetType() == typeof(IsAuthenticatedCondition) || condition.GetType() == typeof(IsAnonymousCondition)
        || condition.GetType() == typeof(UrlCondition) || condition.GetType() == typeof(CultureCondition)
        || condition.GetType() == typeof(RoleCondition) || condition.GetType() == typeof(ContentTypeCondition);

    private static bool IsSupportedOperator(ConditionOperator operation) => operation.GetType() == typeof(StringEqualsOperator)
        || operation.GetType() == typeof(StringNotEqualsOperator) || operation.GetType() == typeof(StringStartsWithOperator)
        || operation.GetType() == typeof(StringNotStartsWithOperator) || operation.GetType() == typeof(StringEndsWithOperator)
        || operation.GetType() == typeof(StringNotEndsWithOperator) || operation.GetType() == typeof(StringContainsOperator)
        || operation.GetType() == typeof(StringNotContainsOperator);

    private JsonObject GetSchema(Condition condition)
    {
        var properties = new JsonObject();
        var required = new JsonArray();
        if (condition is BooleanCondition)
        {
            properties["value"] = new JsonObject { ["type"] = "boolean" };
            required.Add("value");
        }
        else if (condition is JavascriptCondition)
        {
            properties["script"] = new JsonObject { ["type"] = "string", ["minLength"] = 1 };
            required.Add("script");
        }
        else if (condition is DisplayTextConditionGroup)
        {
            properties["displayText"] = new JsonObject { ["type"] = new JsonArray("string", "null") };
        }
        else if (condition is UrlCondition or CultureCondition or RoleCondition or ContentTypeCondition)
        {
            properties["value"] = new JsonObject { ["type"] = "string" };
            properties["operation"] = new JsonObject
            {
                ["type"] = "string",
                ["enum"] = new JsonArray(_operators.Factories.Values.Where(factory => IsSupportedOperator(factory.Create()))
                    .OrderBy(factory => factory.Name, StringComparer.Ordinal).Select(factory => (JsonNode)JsonValue.Create(factory.Name)).ToArray()),
            };
            properties["caseSensitive"] = new JsonObject { ["type"] = "boolean", ["default"] = false };
            required.Add("value");
            required.Add("operation");
        }

        return new JsonObject { ["type"] = "object", ["properties"] = properties, ["required"] = required, ["additionalProperties"] = false };
    }

    private RuleConditionDefinition DescribeCondition(Condition condition)
    {
        var properties = new JsonObject();
        if (!IsSupported(condition))
        {
            properties = JsonSerializer.SerializeToNode(condition, condition.GetType(), JsonSerializerOptions.Web).AsObject();
            properties.Remove("name");
            properties.Remove("conditionId");
            properties.Remove("conditions");
        }
        switch (condition)
        {
            case BooleanCondition boolean: properties["value"] = boolean.Value; break;
            case JavascriptCondition javascript: properties["script"] = javascript.Script; break;
            case DisplayTextConditionGroup group: properties["displayText"] = group.DisplayText; break;
            case UrlCondition url: WriteStringOperation(properties, url.Value, url.Operation); break;
            case CultureCondition culture: WriteStringOperation(properties, culture.Value, culture.Operation); break;
            case RoleCondition role: WriteStringOperation(properties, role.Value, role.Operation); break;
            case ContentTypeCondition contentType: WriteStringOperation(properties, contentType.Value, contentType.Operation); break;
        }

        return new RuleConditionDefinition
        {
            Name = condition.Name,
            ConditionId = condition.ConditionId,
            Properties = properties,
            Conditions = condition is ConditionGroup children ? children.Conditions.Select(DescribeCondition).ToArray() : [],
        };
    }

    private void WriteStringOperation(JsonObject properties, string value, ConditionOperator operation)
    {
        properties["value"] = value;
        properties["operation"] = operation is null ? null : _operators.Factories.Values
            .FirstOrDefault(factory => factory.Create().GetType() == operation.GetType())?.Name;
        properties["caseSensitive"] = operation is StringOperator { CaseSensitive: true };
    }
}
