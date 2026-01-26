using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Abstract base class for defining recipes entirely in code.
/// Subclasses can define recipe steps and schema programmatically without requiring JSON files.
/// </summary>
public abstract class CodeRecipeDescriptor : IRecipeDescriptor
{
    private JsonSchema _schema;

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public virtual string Description => string.Empty;

    /// <inheritdoc />
    public virtual string Author => string.Empty;

    /// <inheritdoc />
    public virtual string WebSite => string.Empty;

    /// <inheritdoc />
    public virtual string Version => "1.0.0";

    /// <inheritdoc />
    public virtual bool IsSetupRecipe => false;

    /// <inheritdoc />
    public virtual DateTime? ExportUtc => null;

    /// <inheritdoc />
    public virtual string[] Categories => [];

    /// <inheritdoc />
    public virtual string[] Tags => [];

    /// <inheritdoc />
    public virtual bool RequireNewScope => true;

    /// <inheritdoc />
    public Task<JsonSchema> GetSchemaAsync()
    {
        _schema ??= BuildSchema();
        return Task.FromResult(_schema);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadStreamAsync()
    {
        var recipe = BuildRecipeJson();
        var memoryStream = new MemoryStream();
        using var writer = new Utf8JsonWriter(memoryStream);
        recipe.WriteTo(writer);
        writer.Flush();
        memoryStream.Position = 0;
        return Task.FromResult<Stream>(memoryStream);
    }

    /// <summary>
    /// Builds the JSON Schema for this recipe.
    /// Override to provide a custom schema. The default implementation builds
    /// a schema from the registered steps.
    /// </summary>
    /// <returns>The <see cref="JsonSchema"/> for this recipe.</returns>
    protected virtual JsonSchema BuildSchema()
    {
        var stepsSchemas = new List<JsonSchema>();

        foreach (var step in BuildSteps())
        {
            var stepSchema = step.Schema;
            if (stepSchema is not null)
            {
                stepsSchemas.Add(stepSchema);
            }
        }

        return new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Title(Name)
            .Description(Description)
            .Type(SchemaValueType.Object)
            .Properties(
                ("name", new JsonSchemaBuilder().Type(SchemaValueType.String).Const(Name)),
                ("displayName", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("author", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("website", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("version", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("issetuprecipe", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                ("categories", new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Type(SchemaValueType.String))),
                ("tags", new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Type(SchemaValueType.String))),
                ("variables", new JsonSchemaBuilder().Type(SchemaValueType.Object).AdditionalProperties(JsonSchema.Empty)),
                ("steps", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(stepsSchemas.Count > 0
                        ? new JsonSchemaBuilder().OneOf(stepsSchemas)
                        : new JsonSchemaBuilder()
                            .Type(SchemaValueType.Object)
                            .Required("name")
                            .Properties(("name", new JsonSchemaBuilder().Type(SchemaValueType.String))))))
            .Required("name", "steps")
            .Build();
    }

    /// <summary>
    /// Builds the complete recipe JSON content.
    /// </summary>
    /// <returns>A <see cref="JsonObject"/> representing the complete recipe.</returns>
    protected virtual JsonObject BuildRecipeJson()
    {
        var recipe = new JsonObject
        {
            ["name"] = Name,
            ["displayName"] = DisplayName,
            ["description"] = Description,
            ["author"] = Author,
            ["website"] = WebSite,
            ["version"] = Version,
            ["issetuprecipe"] = IsSetupRecipe,
        };

        if (Categories?.Length > 0)
        {
            recipe["categories"] = new JsonArray(Categories.Select(c => JsonValue.Create(c)).ToArray());
        }

        if (Tags?.Length > 0)
        {
            recipe["tags"] = new JsonArray(Tags.Select(t => JsonValue.Create(t)).ToArray());
        }

        var variables = BuildVariables();
        if (variables is not null && variables.Count > 0)
        {
            recipe["variables"] = variables;
        }

        var stepsArray = new JsonArray();
        foreach (var step in BuildSteps())
        {
            var stepJson = BuildStepJson(step);
            if (stepJson is not null)
            {
                stepsArray.Add(stepJson);
            }
        }

        recipe["steps"] = stepsArray;

        return recipe;
    }

    /// <summary>
    /// Builds the variables to be included in the recipe.
    /// Override to provide variables that can be referenced in recipe steps.
    /// </summary>
    /// <returns>A <see cref="JsonObject"/> containing the variables, or <c>null</c> if no variables are needed.</returns>
    protected virtual JsonObject BuildVariables() => null;

    /// <summary>
    /// Returns the recipe steps to include in this recipe.
    /// Each step should be an <see cref="IRecipeDeploymentStep"/> that defines
    /// both the schema and execution logic.
    /// </summary>
    /// <returns>An enumerable of recipe deployment steps.</returns>
    protected abstract IEnumerable<IRecipeDeploymentStep> BuildSteps();

    /// <summary>
    /// Builds the JSON representation for a single recipe step.
    /// Override to customize how steps are serialized.
    /// </summary>
    /// <param name="step">The recipe step to serialize.</param>
    /// <returns>A <see cref="JsonObject"/> representing the step.</returns>
    protected virtual JsonObject BuildStepJson(IRecipeDeploymentStep step)
    {
        // Default implementation creates a minimal step object with just the name.
        // Subclasses should override this to provide step-specific data.
        return new JsonObject
        {
            ["name"] = step.Name,
        };
    }
}
