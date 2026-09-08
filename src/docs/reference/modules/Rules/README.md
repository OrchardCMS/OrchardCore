# Rules (`OrchardCore.Rules`)

The Rules module provides a reusable condition model, evaluation services, operators, and admin display drivers for building rules from nested conditions.

Rules are infrastructure. The module does not provide a standalone admin screen or decide what a successful rule does. A consuming feature stores a `Rule`, calls `IRuleService`, and supplies any authorization and management UI around it. The [Layers](../Layers/README.md) module is the built-in consumer: it uses rules to decide which widget layers are active for the current request.

## Enable the module

Enable **Rules** from **Tool** > **Features** in the admin. Enabling a feature that depends on `OrchardCore.Rules`, such as Layers, enables it automatically.

The Rules feature depends on `OrchardCore.Scripting` so that JavaScript conditions can use the scripting infrastructure.

## Rule composition and evaluation

A `Rule` is the root `ConditionGroup`. Conditions directly inside the root are combined with **AND** semantics. A rule succeeds only when every direct child condition succeeds.

Use nested groups to compose more complex expressions:

- **All** (`AllConditionGroup`) succeeds when every child condition succeeds.
- **Any** (`AnyConditionGroup`) succeeds when at least one child condition succeeds.

The root rule and both group types evaluate their children in list order. Root and All groups stop at the first false result. Any groups stop at the first true result.

!!! note
    An empty rule, empty All group, or empty Any group evaluates to `false`. Add a Boolean condition set to `true` when an explicit "always" rule is required.

For example, the following rule represents `(authenticated OR homepage) AND URL starts with /docs`:

```csharp
using OrchardCore.Rules;
using OrchardCore.Rules.Models;

var rule = new Rule
{
    Conditions =
    [
        new AnyConditionGroup
        {
            Conditions =
            [
                new IsAuthenticatedCondition(),
                new HomepageCondition { Value = true },
            ],
        },
        new UrlCondition
        {
            Value = "/docs",
            Operation = new StringStartsWithOperator
            {
                CaseSensitive = false,
            },
        },
    ],
};
```

Conditions can be reordered in consumers that provide a rule editor, such as Layers. Reordering does not change the logical result of built-in conditions, but it can avoid unnecessary work because evaluation short-circuits.

## Built-in conditions

| Condition | Behavior |
| --- | --- |
| **All** | Groups child conditions with AND semantics. |
| **Any** | Groups child conditions with OR semantics. |
| **Boolean** | Returns a fixed `true` or `false` value. This is also useful for an explicit "always" rule. |
| **Homepage** | Tests whether the request path is the tenant home page. The value can invert the result. |
| **URL** | Compares the current request path with a configured value by using a string operator. A leading `~/` is treated as `/`. |
| **Culture** | Compares the current culture name and its parent culture name. For example, `en` can match a current culture of `en-US`. |
| **Role** | Compares the current user's role claims with a configured role name. Negative operators require every role claim to satisfy the negative comparison. |
| **Is authenticated** | Succeeds when the current user is authenticated. |
| **Is anonymous** | Succeeds when the current user is not authenticated. |
| **Content type** | Compares against non-widget content types displayed in `Detail` display mode during the current request. |
| **JavaScript** | Evaluates a JavaScript expression and converts its result to a Boolean. |

URL, Culture, Role, and Content type conditions support these string operators:

- Equals
- Does not equal
- Starts with
- Does not start with
- Ends with
- Does not end with
- Contains
- Does not contain

Each operator has a **Case sensitive** option. Case-insensitive comparisons are the default.

!!! warning
    A Content type condition only observes content items that have already passed through the content display pipeline for the current request. It ignores widgets and display types other than `Detail`. When no applicable content type has been observed, the condition compares an empty string; consequently, a negative operator can succeed.

## JavaScript conditions

The JavaScript condition evaluates its expression through the Orchard Core scripting engine. The expression must produce a value that can be converted to a Boolean. The admin editor validates syntax, runtime errors, and Boolean conversion before saving.

Global methods are supplied by enabled features. When Layers is enabled, it provides:

| Function | Behavior |
| --- | --- |
| `isHomepage()` | Tests whether the request path is the tenant home page. |
| `isAnonymous()` | Tests whether the current user is anonymous. |
| `isAuthenticated()` | Tests whether the current user is authenticated. |
| `isInRole(role)` | Tests a role name case-insensitively. |
| `url(pattern)` | Matches a path case-insensitively. A trailing `*` performs a prefix match. |
| `culture(name)` | Matches the current culture name or its parent culture name case-insensitively. |

```javascript
isAuthenticated() && url("/account*")
```

The URL condition's string operators do not interpret `*` as a wildcard. The trailing-wildcard behavior belongs only to the Layers-provided JavaScript `url()` method.

Rules do not provide Liquid tags, filters, or Liquid rule evaluation. The `[js: ...]` syntax used in recipe files is a recipe expression and is separate from JavaScript conditions.

## Evaluate rules from code

Inject `IRuleService` and call `EvaluateAsync()` with a `Rule`:

```csharp
using OrchardCore.Rules;
using OrchardCore.Rules.Services;

public sealed class RuleGate
{
    private readonly IRuleService _ruleService;

    public RuleGate(IRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    public ValueTask<bool> MatchesAsync(Rule rule)
        => _ruleService.EvaluateAsync(rule);
}
```

The Rules feature registers `IRuleService` and all built-in evaluators. A module that uses these services should declare a feature dependency on `OrchardCore.Rules`.

When using `OrchardCore.Rules.Core` outside the feature, call `services.AddRules()` and register every condition evaluator that a rule can contain. An unregistered condition cannot be evaluated reliably.

## Admin editors and permissions

Condition editors are display drivers and shapes that a consuming module can compose into its own admin UI. The Rules module registers editors for all built-in conditions, but it defines no permissions.

Layers exposes the built-in rule editor and protects its create, edit, delete, and reorder actions with the `ManageLayers` permission. A custom consumer must define and enforce its own permission.

Each condition has a `ConditionId` used by editors to locate, move, update, and delete conditions in a nested tree. Use `IConditionIdGenerator.GenerateUniqueId()` when adding conditions through custom management code.

## Recipes and deployment

Rules do not define a standalone recipe step. Layers serializes its rules as part of the `Layers` step, and the **All Layers** deployment step produces the same structure.

The following valid recipe fragment creates an always-active layer:

```json
{
  "name": "layers",
  "Layers": [
    {
      "Name": "Always",
      "Description": "The widgets in this layer are displayed on any page.",
      "LayerRule": {
        "ConditionId": "[js: uuid()]",
        "Conditions": [
          {
            "$type": "OrchardCore.Rules.Models.BooleanCondition, OrchardCore.Rules",
            "Name": "BooleanCondition",
            "Value": true,
            "ConditionId": "[js: uuid()]"
          }
        ]
      }
    }
  ]
}
```

For conditions directly inside `LayerRule.Conditions`, `Name` must match the registered condition factory name, which is the condition class name by default. The Layers recipe handler uses this name to select the concrete type. `$type` is emitted by polymorphic serialization and is required for conditions nested inside an All or Any group, where the children are deserialized as `Condition` instances. Retain both properties so manually authored recipes and deployment exports use the same round-trippable structure.

Condition identifiers should be unique within the tree; `[js: uuid()]` generates them during recipe execution.

When a `LayerRule` is supplied for an existing layer, recipe execution replaces its complete condition list because nested conditions cannot be merged safely. If a condition name is unknown, the recipe reports an error and does not save the layer changes. Ensure that the feature registering each custom condition is enabled before importing the recipe.

## Create a custom condition

A custom condition consists of:

1. A model derived from `Condition`.
2. An evaluator implementing `IConditionEvaluator`, normally by deriving from `ConditionEvaluator<TCondition>`.
3. Optionally, a display driver and shapes when the condition must be available in an admin rule editor.

This condition tests whether the current request contains a configured header:

```csharp
using Microsoft.AspNetCore.Http;
using OrchardCore.Rules;

public sealed class RequestHeaderCondition : Condition
{
    public string HeaderName { get; set; } = string.Empty;
}

public sealed class RequestHeaderConditionEvaluator
    : ConditionEvaluator<RequestHeaderCondition>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestHeaderConditionEvaluator(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<bool> EvaluateAsync(
        RequestHeaderCondition condition)
    {
        var exists = _httpContextAccessor.HttpContext?.Request.Headers
            .ContainsKey(condition.HeaderName) == true;

        return ValueTask.FromResult(exists);
    }
}
```

Register a condition without an admin editor by using `AddRuleCondition`:

```csharp
services.AddRuleCondition<
    RequestHeaderCondition,
    RequestHeaderConditionEvaluator>();
```

This registration adds the evaluator and factory and registers the concrete condition for polymorphic JSON serialization.

### Add an admin display driver

Derive the driver from `DisplayDriver<Condition, TCondition>`. A complete editor normally provides summary, thumbnail, and edit shapes:

```csharp
using OrchardCore;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules;

public sealed class RequestHeaderConditionViewModel
{
    public string HeaderName { get; set; } = string.Empty;
    public RequestHeaderCondition Condition { get; set; }
}

public sealed class RequestHeaderConditionDisplayDriver
    : DisplayDriver<Condition, RequestHeaderCondition>
{
    public override Task<IDisplayResult> DisplayAsync(
        RequestHeaderCondition condition,
        BuildDisplayContext context)
    {
        return CombineAsync(
            View("RequestHeaderCondition_Fields_Summary", condition)
                .Location(
                    OrchardCoreConstants.DisplayType.Summary,
                    "Content"),
            View("RequestHeaderCondition_Fields_Thumbnail", condition)
                .Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(
        RequestHeaderCondition condition,
        BuildEditorContext context)
    {
        return Initialize<RequestHeaderConditionViewModel>(
            "RequestHeaderCondition_Fields_Edit",
            model =>
            {
                model.HeaderName = condition.HeaderName;
                model.Condition = condition;
            }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(
        RequestHeaderCondition condition,
        UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(
            condition,
            Prefix,
            x => x.HeaderName);

        return Edit(condition, context);
    }
}
```

Place the corresponding Razor shapes under the module's `Views/Items` directory:

```text
RequestHeaderCondition.Fields.Summary.cshtml
RequestHeaderCondition.Fields.Thumbnail.cshtml
RequestHeaderCondition.Fields.Edit.cshtml
```

Register the model, evaluator, and display driver together:

```csharp
services.AddRule<
    RequestHeaderCondition,
    RequestHeaderConditionEvaluator,
    RequestHeaderConditionDisplayDriver>();
```

Do not use the obsolete `AddCondition` extension methods. Use `AddRuleCondition` for a condition without an editor or `AddRule` for a condition with a display driver.

## Create a custom operator

Operators derive from `ConditionOperator`, and their comparers implement `IOperatorComparer`, normally through `OperatorComparer<TOperator, TValue>`. Register the operator in `ConditionOperatorOptions` so editors and `IConditionOperatorResolver` can find its comparer and factory.

The following operator performs an ordinal string comparison:

```csharp
using Microsoft.Extensions.Options;
using OrchardCore.Rules;

public sealed class OrdinalEqualsOperator : ConditionOperator;

public sealed class OrdinalEqualsOperatorComparer
    : OperatorComparer<OrdinalEqualsOperator, string>
{
    public override bool Compare(
        OrdinalEqualsOperator conditionOperator,
        string a,
        string b)
        => string.Equals(a, b, StringComparison.Ordinal);
}

public sealed class OrdinalEqualsOperatorConfigureOptions
    : IConfigureOptions<ConditionOperatorOptions>
{
    public void Configure(ConditionOperatorOptions options)
    {
        options.Operators.Add(
            new ConditionOperatorOption<
                OrdinalEqualsOperatorConfigureOptions>(
                S => S["Equals (ordinal)"],
                new OrdinalEqualsOperatorComparer(),
                typeof(OrdinalEqualsOperator),
                new ConditionOperatorFactory<
                    OrdinalEqualsOperator>()));
    }
}
```

Register the options configuration and the operator's polymorphic JSON type:

```csharp
services.AddTransient<
    IConfigureOptions<ConditionOperatorOptions>,
    OrdinalEqualsOperatorConfigureOptions>();

services.AddRuleConditionOperator<OrdinalEqualsOperator>();
```

The options registration makes the comparer and factory available at runtime. `AddRuleConditionOperator` is separately required for serialization. Without both registrations, evaluation can fail or the concrete operator type and its properties cannot round-trip correctly.

If a negative operator is used with `RoleCondition`, implement the `INegateOperator` marker interface. Role evaluation then requires all role claims to satisfy the negative comparison instead of requiring any one claim to match.

## Integration points

The main extension points are:

| API | Purpose |
| --- | --- |
| `IRuleService` | Evaluates a complete `Rule`. |
| `IConditionEvaluator` / `ConditionEvaluator<T>` | Evaluates a condition type. |
| `IConditionFactory` / `ConditionFactory<T>` | Creates condition instances and supplies their serialized names. |
| `IConditionResolver` | Resolves the evaluator registered for a condition instance. |
| `IConditionIdGenerator` | Assigns identifiers used by rule editors. |
| `IConditionOperatorFactory` | Creates operator instances. |
| `IOperatorComparer` / `OperatorComparer<TOperator, TValue>` | Implements operator comparison. |
| `IConditionOperatorResolver` | Resolves a comparer for an operator instance. |
| `DisplayDriver<Condition, TCondition>` | Supplies admin summary, thumbnail, and edit shapes. |

Layers stores a `Rule` on each `Layer`, evaluates it through `IRuleService` during front-end rendering, exposes condition management through its admin controller, and includes the rule tree in Layers recipes and deployment plans.

## Troubleshooting

### An empty rule never succeeds

This is expected. Add one or more conditions. For an unconditional rule, use a Boolean condition set to `true`.

### A recipe reports an unknown condition

Enable the feature that registers the condition and verify that `Name` matches its `IConditionFactory.Name`. Conditions registered with `AddRule` or `AddRuleCondition` use the condition class name by default.

### A custom condition loses properties after serialization

Register it with `AddRule` or `AddRuleCondition`; both register polymorphic JSON type information. Register every custom operator with `AddRuleConditionOperator`.

### A JavaScript condition cannot be saved

Ensure that the expression is valid JavaScript, that all referenced global methods are registered by enabled features, and that the result can be converted to a Boolean. Layers-specific functions such as `url()` are not supplied by the Rules feature alone.

### A Content type condition does not match

Confirm that the content item is rendered with the `Detail` display type and is not a widget. The evaluator only knows about applicable content types observed during the current request.

### A user cannot manage layer conditions

Grant a role the `ManageLayers` permission. The Rules feature itself has no management permission or standalone admin page.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Iq6VbXZg0B0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
