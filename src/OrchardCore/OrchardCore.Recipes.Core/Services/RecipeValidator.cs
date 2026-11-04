using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <inheritdoc />
public sealed class RecipeValidator : IRecipeValidator
{
    private const string FilePrefix = "file:";

    private readonly IEnumerable<IRecipeStepHandler> _stepHandlers;
    private readonly IEnumerable<IRecipeStepValidator> _stepValidators;

    internal readonly IStringLocalizer S;

    public RecipeValidator(
        IEnumerable<IRecipeStepHandler> stepHandlers,
        IEnumerable<IRecipeStepValidator> stepValidators,
        IStringLocalizer<RecipeValidator> stringLocalizer)
    {
        _stepHandlers = stepHandlers;
        _stepValidators = stepValidators;
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public async Task<RecipeValidationResult> ValidateAsync(RecipeDescriptor recipe, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var context = new RecipeValidationContext { Recipe = recipe };
        var stepCount = 0;

        try
        {
            await using var stream = recipe.RecipeFileInfo.CreateReadStream();
            using var document = await JsonDocument.ParseAsync(stream, JOptions.Document, cancellationToken);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                context.AddError("recipe-not-object", S["The recipe must be a JSON object."]);
            }
            else
            {
                stepCount = await ValidateRootAsync(document.RootElement, context, cancellationToken);
            }
        }
        catch (JsonException exception)
        {
            context.AddError("recipe-invalid-json", S["The recipe isn't valid JSON, at line {0}, position {1}.", exception.LineNumber + 1, exception.BytePositionInLine + 1]);
        }
        catch (IOException)
        {
            context.AddError("recipe-unreadable", S["The recipe file can't be read."]);
        }

        return CreateResult(context.Findings, stepCount);
    }

    /// <inheritdoc />
    public RecipeValidationResult CreateFailure(string code, string message)
        => CreateResult(
        [
            new RecipeValidationFinding
            {
                Severity = RecipeValidationSeverity.Error,
                Code = code,
                Message = message,
            },
        ], 0);

    private RecipeValidationResult CreateResult(ICollection<RecipeValidationFinding> findings, int stepCount)
    {
        var errorCount = findings.Count(finding => finding.Severity == RecipeValidationSeverity.Error);

        return new RecipeValidationResult
        {
            Valid = errorCount == 0,
            Notice = S["A valid result doesn't guarantee that execution succeeds: nothing was applied, scripts weren't evaluated, and a step can still fail on its data."],
            StepCount = stepCount,
            ErrorCount = errorCount,
            WarningCount = findings.Count(finding => finding.Severity == RecipeValidationSeverity.Warning),
            Findings = findings.ToArray(),
        };
    }

    private async Task<int> ValidateRootAsync(JsonElement root, RecipeValidationContext context, CancellationToken cancellationToken)
    {
        // The executor would fail on these variables as soon as it reaches them.
        if (root.TryGetProperty("variables", out var variables) && variables.ValueKind != JsonValueKind.Object)
        {
            context.AddError("recipe-invalid-variables", S["The 'variables' property of a recipe must be an object."]);
        }

        // The executor would do nothing and report a success.
        if (!root.TryGetProperty("steps", out var steps) || steps.ValueKind != JsonValueKind.Array)
        {
            context.AddError("recipe-no-steps", S["A recipe requires a 'steps' array."]);

            return 0;
        }

        var handlers = _stepHandlers.ToArray();
        var stepNames = handlers.SelectMany(handler => handler.StepNames ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasUndeclaredHandler = handlers.Any(handler => handler.StepNames is null);

        var index = 0;
        foreach (var step in steps.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            context.StepIndex = index++;
            context.StepName = null;
            context.Step = null;

            // The executor would fail on this step, after applying the previous ones.
            if (step.ValueKind != JsonValueKind.Object)
            {
                context.AddError("step-not-object", S["A step must be a JSON object."]);

                continue;
            }

            // Without a name, no handler would process this step, and the executor would still report a success.
            if (!step.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(name.GetString()))
            {
                context.AddError("step-no-name", S["A step requires a 'name' string. Without it no handler processes the step, and the execution still succeeds."]);

                continue;
            }

            context.StepName = name.GetString();
            context.Step = JsonObject.Create(step);

            ValidateHandler(context, stepNames, hasUndeclaredHandler);
            await ValidatePropertiesAsync(context);
            ValidateScripts(step, context);
        }

        return index;
    }

    private void ValidateHandler(RecipeValidationContext context, HashSet<string> stepNames, bool hasUndeclaredHandler)
    {
        if (stepNames.Contains(context.StepName))
        {
            return;
        }

        if (hasUndeclaredHandler)
        {
            context.AddWarning("step-handler-unverified", S["No enabled feature declares a handler for the step '{0}', but a registered handler doesn't declare its steps and may process it. If none does, the execution skips this step and still succeeds.", context.StepName]);
        }
        else if (context.FeaturesMayChange || context.PendingFeatures.Count > 0)
        {
            context.AddWarning("step-handler-unverified", S["No enabled feature handles the step '{0}'. An earlier step changes the enabled features, which may provide its handler, but that can't be checked before they are enabled. If none does, the execution skips this step and still succeeds.", context.StepName]);
        }
        else
        {
            context.AddError("unknown-step", S["No enabled feature handles the step '{0}'. The execution would skip this step and still succeed.", context.StepName]);
        }
    }

    private async Task ValidatePropertiesAsync(RecipeValidationContext context)
    {
        foreach (var validator in _stepValidators)
        {
            if (!string.Equals(validator.StepName, context.StepName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                await validator.ValidateAsync(context);
            }
            catch (JsonException)
            {
                context.AddError("step-invalid", S["The properties of the step '{0}' don't have the expected types.", context.StepName]);
            }
        }
    }

    private void ValidateScripts(JsonElement step, RecipeValidationContext context)
    {
        var scripts = 0;
        VisitScripts(step, context, ref scripts);

        if (scripts > 0)
        {
            context.AddInfo("scripts-not-evaluated", S["This step has {0} script expressions, which only the execution evaluates.", scripts]);
        }
    }

    private void VisitScripts(JsonElement element, RecipeValidationContext context, ref int scripts)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    VisitScripts(item, context, ref scripts);
                }

                break;

            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    VisitScripts(property.Value, context, ref scripts);
                }

                break;

            case JsonValueKind.String:
                var value = element.GetString();
                if (!RecipeValidationContext.IsScript(value))
                {
                    break;
                }

                var script = value.Trim('[', ']');
                if (script.StartsWith(FilePrefix, StringComparison.Ordinal))
                {
                    ValidateFileScript(script[FilePrefix.Length..], context);
                }
                else
                {
                    scripts++;
                }

                break;
        }
    }

    // A script of the 'file' engine runs no code, so its file can be checked the way the engine would resolve it.
    private void ValidateFileScript(string script, RecipeValidationContext context)
    {
        string path = null;
        if (script.StartsWith("text('", StringComparison.Ordinal) && script.EndsWith("')", StringComparison.Ordinal))
        {
            path = script[6..^2];
        }
        else if (script.StartsWith("base64('", StringComparison.Ordinal) && script.EndsWith("')", StringComparison.Ordinal))
        {
            path = script[8..^2];
        }

        if (string.IsNullOrEmpty(path))
        {
            context.AddError("file-script-invalid", S["The file script '{0}' isn't supported. Use text('path') or base64('path').", script]);
        }
        else if (!context.Recipe.FileProvider.GetRelativeFileInfo(context.Recipe.BasePath, path).Exists)
        {
            context.AddError("file-not-found", S["The file '{0}' doesn't exist in the recipe's files.", path]);
        }
    }
}
