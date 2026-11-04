using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Rules.Services;

/// <summary>
/// Converts supported rule conditions to a transport-independent management contract.
/// Validation never evaluates a condition or persists a document.
/// </summary>
public interface IRuleManagementService
{
    /// <summary>Gets the registered condition types and their supported management schemas.</summary>
    IReadOnlyList<RuleConditionDescriptor> GetDescriptors();
    /// <summary>Describes the children of a rule, retaining condition identities and extension properties.</summary>
    IReadOnlyList<RuleConditionDefinition> Describe(Rule rule);
    /// <summary>Validates an individual condition without evaluating it. Includes JavaScript required-value and syntax checks shared with the editor.</summary>
    IReadOnlyList<RuleConditionValidationFailure> ValidateCondition(Condition condition);
    /// <summary>Builds a rule from supported definitions, validating properties, structure and identities without persisting it.</summary>
    RuleManagementResult CreateRule(IReadOnlyList<RuleConditionDefinition> conditions, string ruleId = null);
}

/// <summary>A localized condition validation error shared by management APIs and editors.</summary>
public sealed class RuleConditionValidationFailure
{
    /// <summary>Gets the condition property name in the management contract.</summary>
    public string Property { get; init; }
    /// <summary>Gets the localized error message.</summary>
    public string Message { get; init; }
}

/// <summary>A recursive condition definition accepted by the management contract.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class RuleConditionDefinition
{
    /// <summary>Gets the registered condition factory name.</summary>
    public string Name { get; init; }
    /// <summary>Gets an optional stable identity. Missing identities are generated when building a rule.</summary>
    public string ConditionId { get; init; }
    /// <summary>Gets the condition-specific properties described by the live schema.</summary>
    public JsonObject Properties { get; init; } = [];
    /// <summary>Gets the nested conditions. Only group conditions accept children.</summary>
    public IReadOnlyList<RuleConditionDefinition> Conditions { get; init; } = [];
}

/// <summary>Describes a registered condition type and whether its properties can be written through management APIs.</summary>
public sealed class RuleConditionDescriptor
{
    /// <summary>Gets the registered condition factory name.</summary>
    public string Name { get; init; }
    /// <summary>Gets whether this condition type supports management writes.</summary>
    public bool CanWrite { get; init; }
    /// <summary>Gets whether this condition is a group.</summary>
    public bool SupportsChildren { get; init; }
    /// <summary>Gets the writable JSON property schema, when supported.</summary>
    public JsonObject PropertiesSchema { get; init; }
}

/// <summary>Contains a validated rule or property-path validation errors.</summary>
public sealed class RuleManagementResult
{
    /// <summary>Gets the constructed rule, or null when validation fails.</summary>
    public Rule Rule { get; init; }
    /// <summary>Gets validation errors keyed by management property path.</summary>
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
    /// <summary>Gets whether all supplied definitions passed validation.</summary>
    public bool IsValid => Errors.Count == 0;
}
