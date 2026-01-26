using System.Text.Json.Nodes;
using Json.Schema;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Default implementation of <see cref="IRecipeStepSchemaEvaluator"/> using JsonSchema.Net
/// for JSON Schema evaluation capabilities for recipe validation.
/// </summary>
public sealed class RecipeStepSchemaEvaluator : IRecipeStepSchemaEvaluator
{
    private readonly IRecipeSchemaService _schemaService;

    public RecipeStepSchemaEvaluator(IRecipeSchemaService schemaService)
    {
        ArgumentNullException.ThrowIfNull(schemaService);
        _schemaService = schemaService;
    }

    /// <inheritdoc />
    public RecipeSchemaEvaluationResult Evaluate(JsonNode data, JsonSchema schema, EvaluationOptions options = null)
    {
        options ??= new EvaluationOptions { OutputFormat = OutputFormat.List };

        try
        {
            var result = schema.Evaluate(data, options);

            if (result.IsValid)
            {
                return RecipeSchemaEvaluationResult.Success();
            }

            var details = CollectErrorDetails(result);
            return RecipeSchemaEvaluationResult.Failure(details, $"Schema validation failed with {details.Count} error(s).");
        }
        catch (Exception ex)
        {
            return RecipeSchemaEvaluationResult.Failure("/", $"Schema evaluation error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public RecipeSchemaEvaluationResult EvaluateStep(JsonNode stepData, string stepName)
    {
        ArgumentNullException.ThrowIfNull(stepData);
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        var schema = _schemaService.GetStepSchema(stepName);
        if (schema is null)
        {
            // No schema available - consider valid by default.
            return RecipeSchemaEvaluationResult.Success($"No schema available for step '{stepName}'. Skipping validation.");
        }

        return Evaluate(stepData, schema);
    }

    /// <inheritdoc />
    public RecipeSchemaEvaluationResult EvaluateRecipe(JsonNode recipeData)
    {
        ArgumentNullException.ThrowIfNull(recipeData);

        var combinedSchema = _schemaService.GetRecipeSchema();
        return Evaluate(recipeData, combinedSchema);
    }

    private static List<RecipeSchemaEvaluationDetail> CollectErrorDetails(EvaluationResults result)
    {
        var details = new List<RecipeSchemaEvaluationDetail>();
        CollectErrorDetailsRecursive(result, details);
        return details;
    }

    private static void CollectErrorDetailsRecursive(EvaluationResults result, List<RecipeSchemaEvaluationDetail> details)
    {
        if (!result.IsValid)
        {
            if (result.Errors is not null && result.Errors.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    details.Add(new RecipeSchemaEvaluationDetail
                    {
                        InstanceLocation = result.InstanceLocation?.ToString() ?? "/",
                        EvaluationPath = result.EvaluationPath?.ToString() ?? "/",
                        SchemaKeyword = error.Key,
                        Message = error.Value,
                        IsValid = false,
                    });
                }
            }
            else if (!result.HasErrors && result.Details is not null)
            {
                // No direct errors, check nested details.
                foreach (var detail in result.Details)
                {
                    CollectErrorDetailsRecursive(detail, details);
                }
            }
            else if (!result.HasErrors)
            {
                // Generic failure without specific error message.
                details.Add(new RecipeSchemaEvaluationDetail
                {
                    InstanceLocation = result.InstanceLocation?.ToString() ?? "/",
                    EvaluationPath = result.EvaluationPath?.ToString() ?? "/",
                    Message = "Validation failed.",
                    IsValid = false,
                });
            }
        }

        // Always check nested details for any errors.
        if (result.Details is not null)
        {
            foreach (var detail in result.Details)
            {
                if (!detail.IsValid)
                {
                    CollectErrorDetailsRecursive(detail, details);
                }
            }
        }
    }
}
