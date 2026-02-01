# Deployment (`OrchardCore.Deployment`)

Provides features to move content and configuration between Orchard Core sites via [Recipes](../Recipes/README.md). Also see [Remote Deployment](../Deployment.Remote/README.md).

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/wBWa28iHWHI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/2c5pbXuJJb0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

## Overview

The Deployment module enables you to:

- **Export** site content and configuration as deployment packages (recipes).
- **Import** deployment packages to restore or replicate site state.
- Create **Deployment Plans** that define which steps to export.

## JSON Import with Validation

The JSON Import feature (`/Admin/DeploymentPlan/Import/Json`) provides:

- **Monaco Editor** with syntax highlighting.
- **JSON Schema validation** with real-time error detection.
- **IntelliSense** with autocomplete suggestions for recipe steps.

The editor understands all registered recipe steps and provides property suggestions based on their schemas.

## Creating Custom Deployment Steps

### Unified Recipe/Deployment Steps

The recommended approach is to create a unified step that handles both recipe import and deployment export:

```csharp
public sealed class MyDataStep : RecipeDeploymentStep<MyDataStep.StepModel>
{
    private readonly IMyDataService _dataService;

    public MyDataStep(IMyDataService dataService)
    {
        _dataService = dataService;
    }

    public override string Name => "MyData";
    public override string DisplayName => "My Data";
    public override string Description => "Imports and exports custom data.";
    public override string Category => "Custom";

    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Required("name")
            .Properties(
                ("name", new JsonSchemaBuilder().Type(SchemaValueType.String).Const(Name)),
                ("Items", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(new JsonSchemaBuilder()
                        .Type(SchemaValueType.Object)
                        .Properties(
                            ("Id", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                            ("Value", new JsonSchemaBuilder().Type(SchemaValueType.String))))))
            .Build();
    }

    protected override async Task ImportAsync(StepModel model, RecipeExecutionContext context)
    {
        foreach (var item in model.Items ?? [])
        {
            await _dataService.SaveAsync(item);
        }
    }

    protected override async Task<StepModel> BuildExportModelAsync(RecipeExportContext context)
    {
        var items = await _dataService.GetAllAsync();
        return new StepModel { Items = items.ToArray() };
    }

    public sealed class StepModel
    {
        public string Name { get; set; }
        public MyDataItem[] Items { get; set; }
    }
}
```

### Registration

Register your unified step in `Startup.cs`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddRecipeDeploymentStep<MyDataStep>();
}
```

### Adding to Deployment Plans

To make your step available in Deployment Plans (the UI for creating exports), create a deployment step and driver:

```csharp
// The deployment step model.
public class MyDataDeploymentStep : DeploymentStep
{
    public MyDataDeploymentStep()
    {
        Name = "MyData";
    }

    public bool IncludeAll { get; set; } = true;
}

// The deployment source that generates the recipe step.
public class MyDataDeploymentSource : IDeploymentSource
{
    private readonly MyDataStep _step;

    public MyDataDeploymentSource(MyDataStep step)
    {
        _step = step;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
    {
        if (deploymentStep is not MyDataDeploymentStep myStep)
        {
            return;
        }

        await _step.ExportAsync(new RecipeExportContext(result));
    }
}

// The display driver for the deployment step UI.
public class MyDataDeploymentStepDriver : DisplayDriver<DeploymentStep, MyDataDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(MyDataDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("MyDataDeploymentStep_Summary", step).Location("Summary", "Content"),
            View("MyDataDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content"));
    }

    public override IDisplayResult Edit(MyDataDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<MyDataDeploymentStepViewModel>("MyDataDeploymentStep_Edit", model =>
        {
            model.IncludeAll = step.IncludeAll;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(MyDataDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix);
        return Edit(step, context);
    }
}
```

Register deployment components:

```csharp
services.AddDeployment<MyDataDeploymentSource, MyDataDeploymentStep, MyDataDeploymentStepDriver>();
```

## Content Definition Schema Handlers

For content types and parts, you can provide detailed JSON Schema for settings:

### Part Settings Schema

```csharp
public class MyPartSchemaHandler : IContentPartSchemaHandler
{
    public string PartName => "MyPart";

    public JsonSchema BuildSettingsSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("Enabled", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Enable the feature.")),
                ("MaxItems", new JsonSchemaBuilder().Type(SchemaValueType.Integer).Description("Maximum items.")))
            .Build();
    }
}
```

### Field Settings Schema

```csharp
public class MyFieldSchemaHandler : IContentFieldSchemaHandler
{
    public string FieldName => "MyField";

    public JsonSchema BuildSettingsSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                ("Hint", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("Editor hint.")),
                ("Required", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Is required.")))
            .Build();
    }
}
```

### Registration

```csharp
services.AddContentPartSchemaHandler<MyPartSchemaHandler>();
services.AddContentFieldSchemaHandler<MyFieldSchemaHandler>();

// Or when registering the content part/field:
services.AddContentPart<MyPart>()
    .WithSchemaHandler<MyPartSchemaHandler>();

services.AddContentField<MyField>()
    .WithSchemaHandler<MyFieldSchemaHandler>();
```
