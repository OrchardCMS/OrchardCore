# Data Pipelines (`OrchardCore.DataPipelines`)

The Data Pipelines module lets administrators design pipelines that read data, transform it, and deliver it, without writing code. A pipeline can, for example, read the orders of the last 30 days, join them with their customers, compute their totals, write them to an Excel workbook and upload it to an SFTP server every night, or import the rows of a CSV file of the media library as content items.

Pipelines read their data from the [Data Sources](../DataSources/README.md) of the site, and their formulas use the [formula language](../DataSources/README.md#formulas) of the Data Sources module.

## Features

| Feature | Description |
|---------|-------------|
| Data Pipelines (`OrchardCore.DataPipelines`) | The designer, the runs, and the steps that read data sources, transform rows, create and zip files, share download links, and send files to web APIs. Depends on the Data Sources and Users features. |
| Data Pipelines - FTP (`OrchardCore.DataPipelines.Ftp`) | Adds the **Upload to an FTP server** step, with the FluentFTP library. |
| Data Pipelines - SFTP (`OrchardCore.DataPipelines.Sftp`) | Adds the **Upload to an SFTP server** step, with the SSH.NET library. |
| Data Pipelines - Media (`OrchardCore.DataPipelines.Media`) | Adds the **Read a media file** and **Save to the media library** steps. Depends on the Media feature. |
| Data Pipelines - Email (`OrchardCore.DataPipelines.Email`) | Adds the **Send by email** step. Depends on the Email feature. |
| Data Pipelines - Content Items (`OrchardCore.DataPipelines.Contents`) | Adds the **Save content items** step, which creates and updates content items from rows. Depends on the Contents feature. |
| Data Pipelines - Workflows (`OrchardCore.DataPipelines.Workflows`) | Adds the **Run Data Pipeline Task** activity and the **Data Pipeline Run Completed Event**. Depends on the Workflows feature. |

## Concepts

A pipeline is a graph of steps connected by their ports, following the extract, transform, load (ETL) pattern:

1. **Sources** read rows, such as the content items of a content type, the users of the site, the results of a saved query, or the rows of a CSV file.
2. **Transforms** change the rows: they filter, calculate, select, sort, group, join, combine, deduplicate and limit them.
3. **File** steps write the rows to files, such as a CSV file or an Excel workbook, and zip files together.
4. **Destinations** deliver the data: they save files to the media library, share them through download links, upload them to FTP and SFTP servers, send them to web APIs or by email, or save the rows as content items.

Each port carries either **rows**, with typed fields, or **files**. A connection joins an output to an input of the same kind, so a file step sits between the transforms and the destinations that deliver files. An output can be connected to several steps, which each receive all of its data. Some inputs accept several connections, whose data they read one after the other.

The data flows through the pipeline in batches. Every step runs at the same time: a step works on a batch while the step before it reads the next one, and a fast step waits for a slow one, so the memory a run uses doesn't depend on the number of rows. A few steps must see every row before they write any, such as **Sort rows**; they hold the rows they need in memory, up to a [limit](#configuration).

Each step describes the fields it produces from the fields it receives, so the designer knows the fields available to every step, and reports the issues of a pipeline before it runs: a missing setting, a field that doesn't exist, a required input that isn't connected, a loop, or a step whose feature is disabled.

## Designing pipelines

The **Tools** > **Data Pipelines** menu has three pages:

- **Pipelines** lists the pipelines, which can be searched by name. Each one shows its published version, whether it has unpublished changes or is disabled, and the status of its last run. **Edit** opens the designer, and the **Actions** menu runs the pipeline now, opens its run history, or deletes it. **Add Pipeline** creates a pipeline from its name and description, then opens the designer.
- **Run History** lists the runs, see [Run history](#run-history).
- **Shared Files** lists the files shared through download links, see [Download links](#download-links).

The designer has:

- a **toolbox** that lists the available steps by category (Source, Transform, File, Destination), with a search box. Drag a step to the canvas, or click it to add it;
- the **canvas**, which shows the steps and their connections. Drag from an output port to an input port to connect two steps. Clicking an unconnected output port opens a searchable list of the steps that can receive its data (rows or files), and adds the chosen step after it, already connected. An empty pipeline offers its first source. The canvas can be zoomed and fitted to the steps, and changes can be undone and redone (`Ctrl+Z`, `Ctrl+Y`);
- the **step panel**, which opens when a step is selected, over the canvas or pinned below it, with three tabs:
    - **Settings**: the editor of the step, including an optional title shown on the canvas instead of the name of the step. Changes are applied as they are made;
    - **Data**: a preview of the first rows the step produces, see [Previews](#previews);
    - **Fields**: the fields the step receives and produces, with their types. A field's reference, such as `[TitlePart.Title]`, can be copied to use it in a formula;
- the **pipeline panel**, with the **Pipeline** tab (the name and the description of the pipeline, and whether it is enabled), the **Issues** tab, which lists what must be fixed before the pipeline can be published and what deserves a look, and the **History** tab.

### Drafts and versions

The designer edits a **draft** of the pipeline. The draft is saved automatically shortly after each change; when two users edit the same pipeline, the second save is rejected, and the designer offers to reload the other user's changes or to overwrite them.

Runs never execute the draft: they execute the **published version**. **Publish** turns the draft into a new version, numbered 1, 2, and so on. A pipeline with errors can't be published, and a pipeline with warnings asks for a confirmation. Publishing again without changes creates a new version only when another user publishes.

- **Discard draft** drops the changes made since the pipeline was last published.
- **Versions** lists the published versions. A version can be viewed, read-only, and restored, which copies it into the draft; it runs once the draft is published again.

!!! warning
    A published pipeline runs with the access of the user who published it: it reads and writes the data this user may read and write. Publishing a pipeline vouches for what it does, which is why the **Manage data pipelines** permission is security critical.

### Previews

The **Data** tab runs the pipeline up to the selected step, with the draft's settings, and shows the first rows the step produces. A preview:

- reads the data with the access of the user who previews;
- reads at most `PreviewRowLimit` rows (100 by default) from each source;
- never executes destinations. Previewing a destination shows the rows or files it would receive, and a step that comes after a destination, such as the **Rejected** output of **Save content items**, can't be previewed;
- stops after two minutes.

### Runs in the designer

**Run** runs the published version, once the pipeline was published. The **History** tab lists the most recent runs; selecting one shows, on the canvas, the status of each step and the number of rows or files it produced, and follows a queued or running run until it ends. A run can be cancelled from there.

## Steps

### Sources

| Step | Ports | Settings |
|------|-------|----------|
| **Read a data source** (`DataSource`) | Out: rows | The **data set** to read, from any [data source](../DataSources/README.md), such as a content type, the users, or a saved query. The **fields** to read, in order (none reads every field). **Filters** that the rows must all match, with the [operators](../DataSources/README.md#filter-operators) of data sources; dates are written `yyyy-MM-dd` and compared in UTC. The **most rows** to read (0 reads every row). The **parameters** of a data set that takes them, such as a saved query, one `name=value` per line. Parameter and filter values can use [placeholders](#file-names-and-texts), such as `{Parameter:Since}`. |
| **Read a media file** (`ReadMediaFile`) | Out: rows | The path of a CSV, Excel, JSON or JSON Lines **file** of the media library, and its **format** (inferred from the extension by default). Whether the first row holds the names of the columns (CSV, Excel), the CSV **delimiter**, and the **worksheet** of a workbook (the first one by default). Requires the Data Pipelines - Media feature. |

The **Read a data source** step reads the data with the access of the user the run is for: a data set this user may not read is reported as missing. **Read a media file** requires this user to be allowed to manage the media of the file's folder.

### Transforms

| Step | Ports | Settings and behavior |
|------|-------|-----------------------|
| **Filter rows** (`Filter`) | In: rows. Out: **Matched** and **Unmatched** rows. | A **condition**, a [formula](../DataSources/README.md#formulas) that is true or false, such as `[Amount] >= 100 AND [Country] = 'CA'`. The rows that match go to **Matched**, the others to **Unmatched**, which may be left unconnected. A row whose condition can't be evaluated doesn't match, and is reported as a warning. |
| **Add calculated fields** (`CalculatedFields`) | In: rows. Out: rows. | Fields computed by formulas, such as `[Price] * [Quantity]`. A formula can use the fields calculated before it, and a calculated field with the name of an input field replaces its value. A value that can't be calculated for a row is empty, and is reported as a warning. |
| **Select fields** (`SelectFields`) | In: rows. Out: rows. | The fields to keep, in order, each with an optional new name and an optional type to convert its values to. A value that can't be converted becomes empty. Without any field, the rows pass unchanged. |
| **Sort rows** (`Sort`) | In: rows. Out: rows. | The fields to sort by, the most significant first, each ascending or descending. Text is compared ignoring case, and empty values come first. The step holds every row in memory before writing the first one. |
| **Group and summarize** (`Aggregate`) | In: rows. Out: rows. | The fields to **group by** (none summarizes every row into one) and the **values** computed for each group, each with a name, a function (`Count`, `CountDistinct`, `Sum`, `Average`, `Min`, `Max` or `Median`) and the field it reads; a count without a field counts the rows. The output has the group fields followed by the computed values. The step holds one entry per group in memory. |
| **Join** (`Join`) | In: **Left** and **Right** rows. Out: rows. | The **rows to keep**: only the rows that match (inner join), every row of the left input (left join), every row of the right input (right join), or every row of both inputs (full join). The pairs of fields that must hold equal values for two rows to match; a row with an empty key never matches. Whether text keys **ignore case**. The **prefix of right fields**, `Right.` by default, added to a field of the right input whose name the left input already has. The step holds the right input in memory and streams the left one, so connect the smaller data set to the right. |
| **Combine rows** (`Union`) | In: rows, several connections. Out: rows. | Appends the rows of every step connected to its input, one connection after the other. Fields are matched by name: a field some rows don't have is empty in them, and fields of different types get the type that holds both. |
| **Remove duplicates** (`Distinct`) | In: rows. Out: rows. | The fields to **compare** (none compares every field). Keeps the first row of each set of equal values. The step holds one key per distinct row in memory. |
| **Keep first rows** (`Limit`) | In: rows. Out: rows. | The number of rows to **keep**, and of rows to **skip** first. Once it has its rows, the step stops reading, and the steps before it stop too. |

Grouping, deduplicating and joining compare values exactly: numbers compare by value (`2` equals `2.0`), and text compares with its case unless a join ignores it.

### Files

| Step | Ports | Settings |
|------|-------|----------|
| **Create a file** (`CreateFile`) | In: rows. Out: files. | The **format**: CSV, Excel workbook, JSON or JSON Lines, or any other [registered format](../DataSources/README.md#adding-a-file-format). The **file name**, without its extension, `{PipelineName}-{Date:yyyyMMdd-HHmmss}` by default. Whether the file starts with a row of column names, whether the columns are named with the labels of the fields rather than their technical names, the CSV **delimiter** (`tab` for tab-separated values), the **worksheet name** of a workbook, and whether JSON is indented. The rows are written as they arrive, so a large file never needs to fit in memory. |
| **Zip files** (`ZipFiles`) | In: files, several connections. Out: files. | The **archive name**, without the `.zip` extension. Compresses every file it receives into one archive; files with the same name get a number appended. |

### Destinations

| Step | Ports | Settings |
|------|-------|----------|
| **Save to the media library** (`SaveToMedia`) | In: files. | The **folder** of the media library, empty for the root, and whether a file with the same name is **replaced**, or saved under a name with a number appended. The user the run is for must be allowed to manage the media of the folder, and the media library must accept the extension of each file, see [Saving files to the media library](#saving-files-to-the-media-library). Requires the Data Pipelines - Media feature. |
| **Share a download link** (`ShareDownloadLink`) | In: files. | The **recipients**, user names or emails of users of the site, separated by commas. The number of **days the link works**, from 1 to 365 (7 by default). Whether the link is **sent to the recipients by email**, with a **subject** and an optional **message**. See [Download links](#download-links). |
| **Upload to an FTP server** (`UploadToFtp`) | In: files. | The **host** and **port** (21 by default), the **encryption** (explicit TLS, the default, implicit TLS, or none), whether the server's **certificate is validated** (by default), the **user name** and **password**, the **remote folder** (created when it doesn't exist, the user's starting folder when empty), whether existing files are **replaced** (otherwise the step fails when a file exists), and the **timeout** in seconds (30 by default). Requires the Data Pipelines - FTP feature. |
| **Upload to an SFTP server** (`UploadToSftp`) | In: files. | The **host** and **port** (22 by default), the **user name**, a **password**, a **private key** (PEM or OpenSSH format) with its optional **passphrase**, or both, the **host key fingerprint**, the **remote folder** (created when it doesn't exist, the user's home folder when empty), whether existing files are **replaced**, and the **timeout** in seconds (30 by default). See [SFTP host keys](#sftp-host-keys). Requires the Data Pipelines - SFTP feature. |
| **Send to a web API** (`SendToWebApi`) | In: files. | The absolute http or https **URL**, the **method** (`POST` or `PUT`), and the **body**: a `multipart/form-data` form with the file in a field (`file` by default), or the raw content of the file with its media type. Additional **headers**, which aren't secret. The **authentication**: none, basic (user name and password), a bearer token, or an API key in a header (`X-API-Key` by default). The **timeout** of each request in seconds (100 by default). Each file is sent in its own request, and a response with a status code other than 2xx fails the step. |
| **Send by email** (`SendByEmail`) | In: files. | The **To**, **Cc** and **Bcc** recipients, separated by commas or semicolons, the **subject** and the **body**, plain text or HTML, and the **maximum size** of the attachments in megabytes (10 by default; larger files fail the step). Sends one email with every file attached, with the default email provider of the site. By default, no email is sent when there are no files. Requires the Data Pipelines - Email feature. |
| **Save content items** (`SaveContentItems`) | In: rows. Out: **Rejected** rows. | See [Saving content items](#saving-content-items). Requires the Data Pipelines - Content Items feature. |

The destinations record what they delivered, such as the path of a saved file or the URL a file was sent to, in the history of the run.

!!! warning
    The FTP and web API steps warn when the connection isn't encrypted: the credentials and the files then travel in clear text. The web API step sends its requests from the server, to the URL it is given.

The headers of the **Send to a web API** step can't hold credentials: the `Authorization` and `Proxy-Authorization` headers are set with the authentication settings, so they are stored as secrets, and the `Host`, `Content-Length`, `Transfer-Encoding` and `Connection` headers are set by the step. The step uses the `DataPipelines` named `HttpClient`, which can be configured, such as with a proxy, with `services.AddHttpClient("DataPipelines")`.

### Saving content items

The **Save content items** step creates or updates content items of a content type from rows:

- **Rows**: update the item a row matches, or create one; create items, and skip the rows that match one; or update the items rows match, and reject the others.
- **Match on**: the field of the rows that finds the item a row updates, compared with the **content item id** or the **display text** of the latest version of the items. A new item matched by display text gets that display text.
- **Values**: the parts and fields of the items, such as `TitlePart.Title` or `Product.Price`, and the fields of the rows that set them. The parts and fields that can be written are the ones the [content items data source](../DataSources/README.md#content-items) reads. Lists, such as content pickers, take ids separated by commas.
- **Publish the items**, or save them as drafts.
- **Items per save**: the number of items saved to the database at once (100 by default).

Each item is validated before it is saved, as the editor would. The rows that can't be saved go to the **Rejected** output, with the reason in an `Error` field, so they can be written to a file, for example. The user the run is for must be allowed to edit, and to publish when the items are published, items of the content type; a new item is owned by this user.

### File names and texts

The names of the files, the remote folders of the FTP and SFTP steps, the subject and the body of the **Send by email** step, the subject of the email of the **Share a download link** step, and the parameter and filter values of the **Read a data source** step can use these placeholders:

| Placeholder | Value |
|-------------|-------|
| `{PipelineName}` | The name of the pipeline. |
| `{PipelineId}` | The identifier of the pipeline. |
| `{RunId}` | The identifier of the run. |
| `{Date:format}` | When the run started, in the time zone of the site, with a .NET date format such as `yyyy-MM-dd`. `{Date}` uses `yyyy-MM-dd`. |
| `{Parameter:name}` | A parameter of the run, such as one the [Run Data Pipeline Task](#run-data-pipeline-task) passes, or nothing when the run has no parameter of that name. |
| `{FileNames}` | The names of the attached files, in the subject and the body of the **Send by email** step. |

Unknown placeholders are left as they are. The characters a file name can't hold, such as slashes, become underscores.

## Runs

A run executes the published version of a pipeline. A run starts:

- with **Run now**, in the **Actions** menu of the list of pipelines, or **Run**, in the designer, which requires the **Run data pipelines** permission;
- from a workflow, with the **Run Data Pipeline Task**, see [Workflows](#workflows).

A disabled pipeline, or one that was never published, doesn't run.

Runs execute in the background, so a long run neither delays a response nor holds up the other background tasks. A run is queued, then starts once the request that queued it ends; a background task starts, every minute, the queued runs that haven't started yet.

- A run reads and writes data with the access of the user who published the pipeline. When this user no longer exists or is disabled, the run fails until the pipeline is published again.
- One run of a pipeline executes at a time, so two runs don't deliver the same data at once. The other runs of the pipeline wait in the queue.
- A run records its progress every few seconds. A running run that stops reporting its progress for 10 minutes, such as when the site restarted, is marked as failed.
- A step that fails stops the run.
- The files a run creates are kept in a temporary folder of the tenant (`App_Data/Sites/{TenantName}/DataPipelines/Runs/{RunId}`), deleted when the run ends.

### Run history

**Tools** > **Data Pipelines** > **Run History** lists the runs of every pipeline, the most recent first, with their status (queued, running, succeeded, failed or cancelled), their version, when they were queued, how long they took, and what started them and who. The runs can be searched by the name of their pipeline and filtered by status; **Run history**, in the **Actions** menu of a pipeline, lists the runs of that pipeline. The page of a run shows:

- when it was queued, started and ended, and the error that made it fail;
- each step, with its status, the rows and files it read and wrote, and its warnings;
- what it delivered;
- its log, up to its last 200 messages.

A queued or running run can be cancelled, with the **Run data pipelines** permission: a queued run is cancelled at once, and a running run stops at its next batch.

Runs are deleted `RunRetentionDays` days after they ended (30 by default).

## Workflows

The Data Pipelines - Workflows feature adds two activities, in the **Data** category.

### Run Data Pipeline Task

Queues a run of the published version of a pipeline. The run executes in the background, with the access of the user who published the pipeline.

| Setting | Description |
|---------|-------------|
| Data pipeline | The pipeline to run. |
| Parameters | Values passed to the run, one `name=value` per line, as a Liquid template, such as `Since={{ Workflow.Input.Since }}`. Steps read them with the `{Parameter:name}` [placeholder](#file-names-and-texts). |
| Wait for the run to end | Enabled by default. The workflow halts until the run ends, then continues with the **Succeeded** outcome, or with **Failed** when the run failed or was cancelled. The run is the last result, and the `DataPipelineRun` output. Otherwise the workflow continues at once with the **Queued** outcome, and the identifier of the run as the last result. |

The task takes the **Failed** outcome at once when the pipeline doesn't exist, was never published, or is disabled.

### Data Pipeline Run Completed Event

Starts or resumes a workflow when a run ends, such as to notify someone when a run fails.

| Setting | Description |
|---------|-------------|
| Data pipeline | The pipeline whose runs trigger the event, or any pipeline. |
| Runs | Every run, the runs that succeeded, or the runs that failed or were cancelled. |

The run is the last result of both activities: an object with the `RunId`, `PipelineId`, `PipelineName`, `Status` (`Succeeded`, `Failed` or `Cancelled`), `Error` and `CompletedUtc` of the run, its `Deliveries` (each with a `Description` and a `Url`), and its `Steps` (each with a `StepId`, `Title`, `Status`, `RowsIn` and `RowsOut`). The event is triggered with the identifier of the pipeline as its correlation id.

## Security

### Permissions

| Permission | Allows |
|------------|--------|
| `ManageDataPipelines` (Manage data pipelines) | Creating, designing, previewing, publishing and deleting pipelines, and managing the shared files. Security critical: a published pipeline runs with the access of the user who published it. Implies the two other permissions. |
| `RunDataPipelines` (Run data pipelines) | Running published pipelines, and cancelling runs. Implies `ViewDataPipelines`. |
| `ViewDataPipelines` (View data pipelines and their runs) | Listing the pipelines, viewing them and their versions in a read-only designer, and viewing their runs. |

The Administrator role has all three permissions by default.

### Secrets

The passwords, private keys, passphrases, tokens and API keys of steps are encrypted with the [Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/introduction) keys of the tenant. They are never shown again: an editor shows an empty field, which keeps the current secret when left empty, and a checkbox to remove it. The messages of a run never hold secrets.

A secret encrypted on another site, or with other keys, such as after copying a database, can't be read: the step reports an error until the secret is entered again.

### Download links

The **Share a download link** step shares files with users of the site:

- a copy of each file is kept in the `App_Data` folder of the tenant, out of the media library;
- the link holds a random token, of which only a hash is stored. The link is sent to the recipients by email, when the Email feature is enabled and they have an email address, and is never recorded in the run;
- a link works only for its recipients, once signed in, until it expires or is revoked;
- signed in, recipients also find the files shared with them, while their link works, on the **Files shared with you** page, at `/DataPipelines/Files`. The notification email links to this page, which is how recipients without an email address, or sites without the Email feature, get the files.

**Tools** > **Data Pipelines** > **Shared Files** lists the shared files, which can be searched by file or pipeline name, with their recipients, when their link expires and how many times they were downloaded. A link can be revoked, and a file deleted. The content of a file is deleted a day after its link expires.

### SFTP host keys

When the **host key fingerprint** of an **Upload to an SFTP server** step is set, the step refuses to connect to a server that presents another key. The fingerprint is the SHA-256 fingerprint the `ssh` command shows, such as `SHA256:ohD8VZEXGWo6Ez8GSEJQ9WpafgLFsOfLOtGGQCQo6Og` (the `SHA256:` prefix and the Base64 padding are optional). When it is empty, the step warns that the host isn't verified, and the run's log shows the fingerprint the server presented, so it can be copied into the settings.

## Saving files to the media library

The **Save to the media library** step saves only the files whose extension the media library accepts. By default, the media library accepts Excel workbooks (`.xlsx`), but not CSV, JSON, JSON Lines or zip files (`.csv`, `.json`, `.jsonl`, `.zip`); a run that saves such a file fails, and the editor of the step warns about it.

To save these files to the media library, add their extensions to the `AllowedFileExtensions` of the [Media](../Media/README.md#configuration) section. This setting replaces the default list rather than extending it, so it must include every extension the media library should keep accepting:

```json
{
  "OrchardCore": {
    "Media": {
      "AllowedFileExtensions": [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".ico",
        ".webp",
        ".pdf",
        ".doc",
        ".docx",
        ".ppt",
        ".pptx",
        ".pps",
        ".ppsx",
        ".odt",
        ".xls",
        ".xlsx",
        ".psd",
        ".mp3",
        ".m4a",
        ".ogg",
        ".wav",
        ".mp4",
        ".m4v",
        ".mov",
        ".wmv",
        ".avi",
        ".mpg",
        ".ogv",
        ".3gp",
        ".webm",
        ".csv",
        ".json",
        ".jsonl",
        ".zip"
      ]
    }
  }
}
```

!!! warning
    The files of the media library are served to anyone who knows their URL, unless the [Secure Media](../Media/README.md#secure-media) feature restricts them. Use the **Share a download link** step for data that must only reach some users.

## Recipes and deployment

The **All Data Pipelines** deployment step, in the **Data** category of deployment plans, exports every pipeline with its published definition, or its draft when it was never published. The secrets of the steps, the settings whose names start with `Protected`, aren't exported, as they can't be read on another site.

The `DataPipelines` recipe step imports pipelines:

```json
{
  "steps": [
    {
      "name": "DataPipelines",
      "Pipelines": [
        {
          "PipelineId": "4fa5t9rfmxagnt3ab966tzrk5r",
          "Name": "Blog posts export",
          "Description": "Exports the blog posts every night.",
          "IsEnabled": true,
          "Definition": {
            "Steps": [
              {
                "StepId": "source",
                "Type": "DataSource",
                "X": 180,
                "Y": 320,
                "Properties": {
                  "DataSourceStepSettings": { "Source": "Contents", "DataSet": "BlogPost" }
                }
              },
              {
                "StepId": "file",
                "Type": "CreateFile",
                "X": 480,
                "Y": 320,
                "Properties": {
                  "CreateFileStepSettings": { "Format": "xlsx", "FileName": "posts" }
                }
              }
            ],
            "Connections": [
              { "SourceStepId": "source", "SourcePort": "Output", "TargetStepId": "file", "TargetPort": "Input" }
            ]
          }
        }
      ]
    }
  ]
}
```

A pipeline is matched by its identifier, at most 26 characters: an existing pipeline gets the imported name, description and definition as its draft, and keeps its published version; otherwise a pipeline is created, with a new identifier when none is given. The settings of a step are in its `Properties`, under the name of their type, as an export shows them. An imported definition is a draft: it runs once a user publishes it, as runs execute with the access of the user who published the pipeline. Enter the secrets of the steps again before publishing.

## Configuration

The limits of the engine are the `DataPipelineOptions`, bound to the `OrchardCore:DataPipelines` section of the configuration, for all tenants or per tenant:

```json
{
  "OrchardCore": {
    "DataPipelines": {
      "MaxRowsInMemory": 1000000,
      "RunRetentionDays": 30,
      "PreviewRowLimit": 100
    }
  }
}
```

| Option | Default | Description |
|--------|---------|-------------|
| `MaxRowsInMemory` | `1000000` | The number of rows a step that must see every row before it writes any may hold in memory: the rows of **Sort rows** and of the right input of **Join**, the groups of **Group and summarize**, and the distinct rows of **Remove duplicates**. A run fails rather than exhausting the memory of the server. |
| `RunRetentionDays` | `30` | The number of days the runs are kept after they end. `0` keeps them. |
| `PreviewRowLimit` | `100` | The number of rows each source reads when a step is previewed. |

The options can also be set from code:

```csharp
services.Configure<DataPipelineOptions>(options => options.MaxRowsInMemory = 5_000_000);
```

## Extending data pipelines

### Adding a data source or a file format

The **Read a data source** step reads every registered data source, and the **Create a file** and **Read a media file** steps offer every registered file format. See [Extending data sources](../DataSources/README.md#extending-data-sources).

### Adding a step type

A step type is a stateless, scoped service; the settings of each step are stored on its `DataPipelineStep`. Derive from `DataPipelineStepType<TSettings>`, from the `OrchardCore.DataPipelines.Abstractions` package, to store the settings as one object, read with `GetSettings(step)`:

- `Category` sets the default ports: a `Source` has one rows output; a `Transform` has one required rows input and one rows output; a `File` step has one required rows input and one files output; a `Destination` has one required files input. Override `GetInputs` and `GetOutputs` for other ports, such as the two inputs of **Join**. Destinations are never executed by a preview.
- `DescribeAsync` checks the settings against the fields of the inputs, reports errors and warnings, and sets the fields of each rows output. The designer calls it to list the fields available to the next steps, and the engine calls it before a run. By default, a step passes the fields of its input to its outputs.
- `ExecuteAsync` reads the inputs and writes the outputs until the inputs are exhausted. It runs at the same time as the other steps, in its own scope.

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DataSources;

public sealed class MaskTextStepSettings
{
    public List<string> Fields { get; set; } = [];

    public int VisibleCharacters { get; set; } = 4;
}

public sealed class MaskTextStep : DataPipelineStepType<MaskTextStepSettings>
{
    public const string StepName = "MaskText";

    private readonly IStringLocalizer S;

    public MaskTextStep(IStringLocalizer<MaskTextStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Mask text"];

    public override LocalizedString Description => S["Hides all but the last characters of text fields."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Transform;

    public override string Icon => "fa-solid fa-mask";

    public override Task DescribeAsync(DataPipelineDescribeContext context)
    {
        var fields = context.GetInputFields();
        var settings = GetSettings(context.Step);

        if (settings.Fields.Count == 0)
        {
            context.AddWarning(S["Choose the fields to mask."]);
        }

        foreach (var name in settings.Fields)
        {
            var field = fields.Find(name);

            if (field is null)
            {
                context.AddError(S["The input has no field named '{0}'.", name]);
            }
            else if (field.Type != DataFieldType.Text)
            {
                context.AddError(S["The field '{0}' isn't text.", name]);
            }
        }

        // The step doesn't change the fields of the rows.
        context.SetOutputFields(fields);

        return Task.CompletedTask;
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        var settings = GetSettings(context.Step);
        var output = context.GetOutput();

        await foreach (var batch in context.GetInput().ReadBatchesAsync(context.CancellationToken))
        {
            // Every step connected to the output is done reading, such as a step that keeps the first rows only.
            if (output.IsClosed)
            {
                return;
            }

            var indexes = settings.Fields.Select(batch.IndexOf).Where(index => index >= 0).ToArray();
            var rows = new List<object[]>(batch.Count);

            foreach (var source in batch.Rows)
            {
                // Rows may be shared with the other steps connected to the same output: copy before changing them.
                var row = (object[])source.Clone();

                foreach (var index in indexes)
                {
                    if (row[index] is string { Length: > 0 } text && text.Length > settings.VisibleCharacters)
                    {
                        row[index] = new string('*', text.Length - settings.VisibleCharacters) + text[^settings.VisibleCharacters..];
                    }
                }

                rows.Add(row);
            }

            // Waits while the steps downstream are busy.
            await output.WriteAsync(new DataBatch(batch.Fields, rows), context.CancellationToken);
        }
    }
}
```

The `DataPipelineStepContext` gives a step:

- its inputs and outputs, with `GetInput(port)` and `GetOutput(port)` (`Input` and `Output` by default). An input reads rows with `ReadBatchesAsync` or files with `ReadFilesAsync`; an output writes rows with `WriteAsync` or files with `WriteFileAsync`. The `Fields` of an input or an output are the fields described before the run;
- the `Run`: the user the run is for (`Run.User`), its parameters, whether it is a preview (`Run.IsPreview`), and `Run.CreateFile(fileName, contentType)`, which creates a file in the temporary folder of the run;
- the `Services` of the step's scope, and the `CancellationToken` that is cancelled when the run is cancelled or fails;
- `LogInformation` and `LogWarning`, which record messages in the history of the run, and `AddDelivery`, which records what a destination delivered. These messages must not hold secrets.

The engine counts the rows and files each step reads and writes.

!!! note
    A step reads and writes data for the user the run is for, `Run.User`: the user who published the pipeline, or the designer during a preview. A step that reads or writes data of the site must check this user's permissions, as the built-in steps do.

### Adding the editor of a step type

A step's editor in the designer is rendered by a display driver of `DataPipelineStep`. Derive from `DataPipelineStepDisplayDriver<TSettings, TViewModel>`, from the `OrchardCore.DataPipelines` module, which renders:

- the `{StepName}_Fields_Edit` shape, the editor in the **Settings** tab, from a `{StepName}.Fields.Edit.cshtml` view;
- the `{StepName}_Fields_Design` shape, the summary of the step on the canvas, from a `{StepName}.Fields.Design.cshtml` view.

```csharp
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DisplayManagement.Handlers;

public sealed class MaskTextStepDisplayDriver : DataPipelineStepDisplayDriver<MaskTextStepSettings, MaskTextStepViewModel>
{
    protected override string StepName => MaskTextStep.StepName;

    protected override ValueTask EditAsync(DataPipelineStep step, MaskTextStepSettings settings, MaskTextStepViewModel model)
    {
        model.Fields = string.Join(", ", settings.Fields);
        model.VisibleCharacters = settings.VisibleCharacters;

        return ValueTask.CompletedTask;
    }

    protected override ValueTask UpdateAsync(DataPipelineStep step, MaskTextStepSettings settings, MaskTextStepViewModel model, UpdateEditorContext context)
    {
        settings.Fields = (model.Fields ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        settings.VisibleCharacters = Math.Max(0, model.VisibleCharacters);

        return ValueTask.CompletedTask;
    }
}

public class MaskTextStepViewModel
{
    public string Fields { get; set; }

    public int VisibleCharacters { get; set; }
}
```

`MaskText.Fields.Edit.cshtml`:

```razor
@model MaskTextStepViewModel

<div class="ocat-wrapper" asp-validation-class-for="Fields">
    <label asp-for="Fields" class="ocat-label">@T["Fields"]</label>
    <div class="ocat-end">
        <input asp-for="Fields" class="form-control" />
        <span asp-validation-for="Fields"></span>
        <span class="hint">@T["The text fields to mask, separated by commas."]</span>
    </div>
</div>

<div class="ocat-limited-wrapper" asp-validation-class-for="VisibleCharacters">
    <label asp-for="VisibleCharacters" class="ocat-label">@T["Visible characters"]</label>
    <div class="ocat-limited">
        <input asp-for="VisibleCharacters" type="number" min="0" class="form-control" />
        <span asp-validation-for="VisibleCharacters"></span>
    </div>
</div>
```

`MaskText.Fields.Design.cshtml`:

```razor
@model MaskTextStepViewModel

<span>@T["Masks {0}", Model.Fields]</span>
```

Report invalid values with `context.Updater.ModelState.AddModelError(Prefix, nameof(model.Property), message)` in `UpdateAsync`. Register the step type and its driver together, in a module that depends on the `OrchardCore.DataPipelines` feature:

```csharp
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataPipelineStep<MaskTextStep, MaskTextStepDisplayDriver>();
    }
}
```

`AddDataPipelineStepType<TStepType>()`, from the `OrchardCore.DataPipelines.Core` package, registers a step type without an editor.

### Reacting to the end of runs

Implement `IDataPipelineRunHandler` to react when a run ends, whether it succeeded, failed or was cancelled. Handlers are called in their own scope, after the run is saved; this is how the Workflows feature resumes the workflows that wait for a run.

```csharp
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;

public sealed class FailedRunHandler : IDataPipelineRunHandler
{
    private readonly ILogger _logger;

    public FailedRunHandler(ILogger<FailedRunHandler> logger)
    {
        _logger = logger;
    }

    public Task CompletedAsync(DataPipelineRun run)
    {
        if (run.Status == DataPipelineRunStatus.Failed)
        {
            _logger.LogWarning("The data pipeline '{PipelineName}' failed: {Error}", run.PipelineName, run.Error);
        }

        return Task.CompletedTask;
    }
}
```

```csharp
services.AddScoped<IDataPipelineRunHandler, FailedRunHandler>();
```
