# Data Orchestrator (`OrchardCore.DataOrchestrator`)

The ETL (Extract, Transform, Load) module provides a visual pipeline editor for building data integration workflows. It allows users to extract data from various sources, transform it using configurable operations, and load it into different destinations.

The module adds the **Data Pipelines** admin section, where editors can design pipelines on a visual canvas, configure each activity, run pipelines manually, inspect execution logs, and optionally schedule enabled pipelines.

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

## Feature Dependencies

The base `OrchardCore.DataOrchestrator` feature provides the visual pipeline designer, pipeline execution services, scheduling support, transforms, file export formats, JSON and Excel sources, and file-based destinations.

Some activities are registered only when their dependency feature is enabled:

| Activity | Required feature | Notes |
|----------|------------------|-------|
| Query Source | `OrchardCore.Queries` | Lists available saved queries in the activity editor and requires selecting one. |
| Content Item Source | `OrchardCore.Contents` | Reads content items and uses content type definitions for editor choices. |
| Content Item Load | `OrchardCore.Contents` | Creates or updates content items. |

## Built-in Activities

### Sources

#### Content Item Source

Extracts content items from the Orchard Core content store.
This activity is available when the `OrchardCore.Contents` feature is enabled.

| Setting | Description |
|---------|-------------|
| Content Type | The content type to extract (e.g., `Article`, `BlogPost`). |
| Version Scope | `Published`, `Latest`, or `All Versions`. |
| Owner | Optional owner username filter. |
| Created From / To (UTC) | Optional creation date range filter. |

#### Query Source

Executes a named query registered by the **Queries** feature (SQL, Lucene, or Elasticsearch) and streams its results as records.
This activity is available when the `OrchardCore.Queries` feature is enabled, and the query selection is required.

| Setting | Description |
|---------|-------------|
| Query | The existing query to run. |
| Parameters | Optional JSON object of parameters passed to the query. |

This activity uses the `IQueryManager` service. When no query with the selected name exists, the source yields no records.

#### Excel Workbook Source

Reads rows from an `.xlsx` file stored under the tenant's Data Orchestrator folder in App_Data. The editor can upload a workbook or reference an existing tenant-relative file path; paths are sandboxed and cannot escape App_Data.

Uploaded workbooks are saved under the tenant's Data Orchestrator folder, e.g. `App_Data/Sites/{TenantName}/DataOrchestrator/Uploads`. Existing file paths must be relative to `App_Data/Sites/{TenantName}/DataOrchestrator` and must not contain path traversal segments.

| Setting | Description |
|---------|-------------|
| Upload Excel File | Uploads an `.xlsx` workbook into the tenant's Data Orchestrator uploads folder. |
| Excel File Path | A path relative to the tenant's Data Orchestrator App_Data folder (e.g., `Uploads/products.xlsx`). |
| Worksheet Name | The worksheet to read. Leave empty for the first one. |
| First row contains headers | Whether to use the first row as column names. |

#### JSON Source

Reads records from a JSON array provided inline.

| Setting | Description |
|---------|-------------|
| JSON Data | A JSON array of objects to use as input. |

### Transforms

#### Field Mapping

Selects and renames fields using `source.path` → `target` mappings, producing a reshaped record that contains only the mapped fields.

| Setting | Description |
|---------|-------------|
| Mappings | A JSON array of `{ "source": "from.path", "target": "ToField" }` entries. |

#### Filter

Keeps only the records that match a field condition.

| Setting | Description |
|---------|-------------|
| Field | The (dotted) field path to evaluate. |
| Operator | `equals`, `not_equals`, `contains`, `starts_with`, `greater_than`, `is_empty`, ... |
| Value | The value to compare against. |

#### Format Value

Formats or converts a field value (currency, number, date/time, UTC-to-time-zone conversion, upper/lower case) and writes it back to the record.

| Setting | Description |
|---------|-------------|
| Field / Output Field | The source field and the field the formatted value is written to. |
| Format Type | `Currency`, `Number`, `DateTime`, `ConvertUtcToTimeZone`, `Uppercase`, `Lowercase`. |
| Format String / Culture / Time Zone | Optional format options. |

#### Join Data Sets

Merges the current data stream with a second data set on a key field.

### Loads (Destinations)

Load activities are the pipeline's destinations. File-based destinations share a common set of **export formats** (see [Export Formats](#export-formats)), so you choose *where* to write independently from *how* the data is serialized.

#### Media Folder Export

Writes the serialized data to a file in the tenant's media library.

| Setting | Description |
|---------|-------------|
| Format | `JSON`, `CSV`, or `Excel Workbook`. |
| File Name | The media path the file is written to (e.g., `exports/output.json`). |

#### FTP / FTPS Export

Uploads the serialized data to an FTP or FTPS server.
The password is stored protected using ASP.NET Core data protection and is not re-displayed in the activity editor after saving.

| Setting | Description |
|---------|-------------|
| Host / Port | The FTP server address. |
| User Name / Password | Credentials used to authenticate. |
| Remote Directory | The target directory (created when missing). |
| Security | `Auto`, `Explicit FTPS`, `Implicit FTPS`, or `None`. |
| Format | `JSON`, `CSV`, or `Excel Workbook`. |
| File Name | The name of the uploaded file. |
| Passive mode | Recommended when the client is behind a firewall/NAT. |
| Accept any TLS certificate | Allow self-signed certificates. |

#### Content Item Load

Creates or updates content items in the Orchard Core content store.
This activity is available when the `OrchardCore.Contents` feature is enabled.

| Setting | Description |
|---------|-------------|
| Content Type | The content type to create or update. |

### Export Formats

File-based destinations serialize records using a registered **export format**. The following formats are built in:

| Format | Extension | Description |
|--------|-----------|-------------|
| JSON | `.json` | An indented JSON array of records. |
| CSV | `.csv` | A comma-separated file; the header is the union of every record's fields. |
| Excel Workbook | `.xlsx` | A single-worksheet workbook. |

Formats are extensible — see [Extending the ETL Module](#extending-the-etl-module).

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
- **Status** — `Running`, `Success`, `Failed`, or `Cancelled`.
- **Records Processed / Loaded** — Number of records that flowed through the pipeline and that were written to a destination.
- **Errors** — Any errors encountered during execution, with details.

View logs from the **Logs** link on the pipeline list page.

## Extending the ETL Module

The ETL module is designed to be extended at three levels: **sources**, **destinations**, and **export formats**. Each is registered from a module's `Startup.cs`.

### Creating a Custom Source

Derive from `EtlSourceActivity`, set `context.DataStream`, and return an outcome:

```csharp
public sealed class SqlServerSource : EtlSourceActivity
{
    public override string Name => nameof(SqlServerSource);

    public override string DisplayText => "SQL Server";

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

    public override IEnumerable<EtlOutcome> GetPossibleOutcomes()
    {
        return [new EtlOutcome("Done")];
    }

    public override Task<EtlActivityResult> ExecuteAsync(EtlExecutionContext context)
    {
        // Implementation: query SQL Server and stream the rows as records.
        context.DataStream = QuerySqlServerAsync(ConnectionString, Query, context.CancellationToken);

        return Task.FromResult(Outcomes("Done"));
    }
}
```

The base `Category` (`Sources`, `Transforms`, or `Loads`) is provided by the `EtlSourceActivity`, `EtlTransformActivity`, and `EtlLoadActivity` base classes.

### Creating a Custom Destination

A destination is a load activity. Derive from `EtlFileExportLoad` to reuse the built-in export formats (JSON/CSV/Excel) and only implement where the file is written:

```csharp
public sealed class S3ExportLoad : EtlFileExportLoad
{
    public override string Name => nameof(S3ExportLoad);

    public override string DisplayText => "Amazon S3";

    public string Bucket
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    protected override async Task WriteToDestinationAsync(
        EtlExecutionContext context, string fileName, Stream content, IEtlExportFormat format)
    {
        // Upload `content` (already serialized in the selected format) to S3.
    }
}
```

For a destination that is not file-based (for example a reporting server such as Tableau, or another content store), derive from `EtlLoadActivity` directly and consume `context.DataStream`.

### Creating a Custom Export Format

Implement `IEtlExportFormat` to add a serialization format that immediately becomes available to every file-based destination:

```csharp
public sealed class XmlExportFormat : IEtlExportFormat
{
    public string Name => "Xml";

    public string DisplayText => "XML";

    public string FileExtension => "xml";

    public string MimeType => "application/xml";

    public async Task WriteAsync(IAsyncEnumerable<JsonObject> records, Stream output, CancellationToken cancellationToken)
    {
        // Serialize `records` to `output` as XML.
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
        // A source, transform, or destination activity (with its display driver).
        services.AddEtlActivity<SqlServerSource, SqlServerSourceDisplayDriver>();
        services.AddEtlActivity<S3ExportLoad, S3ExportLoadDisplayDriver>();

        // An export format available to every file-based destination.
        services.AddEtlExportFormat<XmlExportFormat>();
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

The module does not require any `appsettings.json` configuration. Scheduled execution is driven by each pipeline's own cron **Schedule** (set in the pipeline properties); a background task evaluates enabled, scheduled pipelines every 10 minutes and runs the ones that are due.

Local Excel workbooks are stored under each tenant's App_Data folder in `Sites/{TenantName}/DataOrchestrator`.

The FTP / FTPS destination stores its connection settings (host, user name, protected password, directory, and security mode) on the activity itself, as part of the pipeline definition.

## Videos

<div class="video-container"></div>

## Resources

- [Workflows Module](../Workflows/README.md) — The ETL pipeline editor shares architectural patterns with the Workflows visual editor.
- [Background Tasks](../BackgroundTasks/README.md) — Scheduled pipeline execution uses the background tasks infrastructure.
- [Contents](../Contents/README.md) — The Content Item Source and Load activities interact with the content management system.
