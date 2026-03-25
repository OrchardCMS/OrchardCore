# Data Orchestrator (`OrchardCore.DataOrchestrator`)

The ETL (Extract, Transform, Load) module provides a visual pipeline editor for building data integration workflows. It allows users to extract data from various sources, transform it using configurable operations, and load it into different destinations.

## General Concepts

An ETL pipeline is a collection of **activities** connected by **transitions**. Activities are categorized into three types:

- **Sources** (Extract): Produce data streams from content items, JSON files, APIs, or custom providers.
- **Transforms**: Modify, filter, map, or reshape data as it flows through the pipeline.
- **Loads** (Destinations): Consume data and write it to files, content items, APIs, or external services.

Pipelines are defined visually using a drag-and-drop editor (similar to the Workflows editor) and can be executed manually, on a schedule, or on demand with parameters.

### Pipeline Flow

```
Source(s) → Transform(s) → Load(s)
```

A pipeline can have:

- **Multiple sources** that feed data into the pipeline.
- **Multiple transforms** chained together to progressively reshape data.
- **Multiple loads** (fan-out) to send data to different destinations simultaneously.

## Vocabulary

### Pipeline Definition

A document that contains all information about an ETL pipeline: its name, activities, transitions, parameters, schedule, and enabled status.

### Activity

A step in a pipeline. Each activity has a **type** (Source, Transform, or Load), configurable **properties**, and one or more **outcomes** that connect to downstream activities via transitions.

### Transition

A connection from one activity's outcome to another activity. Transitions define the order of execution and data flow.

### Pipeline Parameter

A named input value that can be provided when executing a pipeline on demand. Parameters support types like `String`, `Number`, `Date`, and `Boolean`, with optional default values.

### Execution Log

A record of a pipeline run, including start/end times, status (Running, Completed, Failed), records processed, and any errors encountered.

## Pipeline Editor

The pipeline editor provides a visual canvas for designing ETL pipelines:

1. **Activity Picker** — Add sources, transforms, and loads from categorized lists.
2. **Canvas** — Drag and position activities, then connect them via transitions.
3. **Activity Editor** — Click an activity to configure its properties.
4. **Properties Panel** — Set pipeline name, parameters, schedule, and enabled status.

Activities are color-coded:

- 🟢 **Green** — Sources
- 🔵 **Blue** — Transforms
- 🔴 **Red** — Loads

## Built-in Activities

### Sources

#### Content Item Source

Extracts content items from the Orchard Core content store.

| Setting | Description |
|---------|-------------|
| Content Type | The content type to query (e.g., `Article`, `BlogPost`). |
| Query | Optional Lucene/Elasticsearch query string to filter items. |
| Max Items | Maximum number of items to extract (0 = unlimited). |

#### JSON Source

Reads data from a JSON string or file.

| Setting | Description |
|---------|-------------|
| JSON Data | Raw JSON array or object to use as input. |
| Source Path | JSONPath expression to select data within the JSON structure. |

### Transforms

#### Field Mapping Transform

Maps fields from the source schema to a target schema using JSONPath or Liquid expressions.

| Setting | Description |
|---------|-------------|
| Mappings | A JSON object mapping target field names to source expressions. |

Example mappings:

```json
{
    "Title": "$.ContentItem.DisplayText",
    "Author": "$.ContentItem.Owner",
    "PublishedDate": "{{ ContentItem.PublishedUtc | date: '%Y-%m-%d' }}"
}
```

#### Filter Transform

Filters records based on a condition expression.

| Setting | Description |
|---------|-------------|
| Expression | A Liquid expression that evaluates to `true` or `false`. |

Example expression:

```liquid
{{ Record.Status == "Published" }}
```

### Loads

#### JSON Export Load

Exports data as a JSON file.

| Setting | Description |
|---------|-------------|
| File Path | The output file path (relative to the tenant's data folder). |
| Format | `Array` (default) or `Lines` (JSON Lines format). |

#### Content Item Load

Creates or updates content items in the Orchard Core content store.

| Setting | Description |
|---------|-------------|
| Content Type | The content type to create. |
| Owner | The owner to assign to created items. |
| Published | Whether to publish items immediately. |

## Pipeline Parameters

Pipelines can define parameters that are provided at execution time. This is useful for:

- **Date ranges**: Filter data by a time period.
- **Content types**: Make pipelines reusable across different content types.
- **Output paths**: Specify where to export data.

Parameters are defined in the pipeline properties and can be referenced in activity expressions:

```liquid
{{ Parameters.StartDate }}
{{ Parameters.ContentType }}
```

### Parameter Types

| Type | Description |
|------|-------------|
| `String` | Text value |
| `Number` | Numeric value |
| `Date` | Date/time value |
| `Boolean` | True/false value |

## Execution

### Manual Execution

Pipelines can be executed manually from the admin UI by clicking the **Execute** button on the pipeline list or editor page.

### Scheduled Execution

Pipelines can run on a schedule using Orchard Core's background task system. Set the **Schedule** property in the pipeline properties to a cron expression:

| Schedule | Cron Expression |
|----------|----------------|
| Every hour | `0 * * * *` |
| Daily at midnight | `0 0 * * *` |
| Every Monday at 9 AM | `0 9 * * 1` |
| Every 15 minutes | `*/15 * * * *` |

Only **enabled** pipelines with a schedule are executed automatically.

### On-Demand with Parameters

Pipelines with parameters can be executed on demand, prompting the user to provide parameter values before running.

## Execution Logs

Each pipeline execution is logged with:

- **Start/End time** — When the pipeline started and finished.
- **Status** — `Running`, `Completed`, or `Failed`.
- **Records Processed** — Number of records that flowed through the pipeline.
- **Errors** — Any errors encountered during execution, with details.

View logs from the **Logs** link on the pipeline list page.

## Extending the ETL Module

The ETL module is designed to be extensible. Developers can add custom sources, transforms, and loads by implementing activity classes and registering them in their module's `Startup.cs`.

### Creating a Custom Source

```csharp
public sealed class SqlServerSource : EtlSourceActivity
{
    public override string Name => nameof(SqlServerSource);
    public override LocalizedString DisplayText => S["SQL Server Source"];
    public override LocalizedString Category => S["Sources"];

    public string ConnectionString
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public string Query
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes(
        IStringLocalizer localizer)
    {
        return Outcomes(localizer["Done"]);
    }

    public override async Task<EtlActivityResult> ExecuteAsync(
        EtlExecutionContext context)
    {
        // Implementation: query SQL Server and yield records
        var records = QuerySqlServerAsync(ConnectionString, Query);
        context.DataStream = records;
        return EtlActivityResult.Success(["Done"]);
    }
}
```

### Creating a Display Driver

```csharp
public sealed class SqlServerSourceDisplayDriver
    : EtlActivityDisplayDriver<SqlServerSource, SqlServerSourceViewModel>
{
    protected override void EditActivity(
        SqlServerSource activity,
        SqlServerSourceViewModel model)
    {
        model.ConnectionString = activity.ConnectionString;
        model.Query = activity.Query;
    }

    protected override void UpdateActivity(
        SqlServerSourceViewModel model,
        SqlServerSource activity)
    {
        activity.ConnectionString = model.ConnectionString;
        activity.Query = model.Query;
    }
}
```

### Registering in Startup.cs

```csharp
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddEtlActivity<SqlServerSource, SqlServerSourceDisplayDriver>();
    }
}
```

### Creating View Templates

Each activity needs three Razor views:

- `Views/Items/{ActivityName}.Fields.Thumbnail.cshtml` — Card shown in the activity picker.
- `Views/Items/{ActivityName}.Fields.Design.cshtml` — Summary shown on the canvas.
- `Views/Items/{ActivityName}.Fields.Edit.cshtml` — Editor form for configuring the activity.

## Permissions

| Permission | Description |
|------------|-------------|
| `ManageEtlPipelines` | Create, edit, and delete ETL pipeline definitions. |
| `ExecuteEtlPipelines` | Execute ETL pipelines manually. |
| `ViewEtlPipelines` | View ETL pipeline definitions and execution logs. |

## Configuration

The ETL module can be configured via `appsettings.json`:

```json
{
    "OrchardCore": {
        "OrchardCore_ETL": {
            "MaxConcurrentPipelines": 5,
            "DefaultTimeout": "00:30:00"
        }
    }
}
```

## Videos

<div class="video-container"></div>

## Resources

- [Workflows Module](../Workflows/README.md) — The ETL pipeline editor shares architectural patterns with the Workflows visual editor.
- [Background Tasks](../BackgroundTasks/README.md) — Scheduled pipeline execution uses the background tasks infrastructure.
- [Contents](../Contents/README.md) — The Content Item Source and Load activities interact with the content management system.
