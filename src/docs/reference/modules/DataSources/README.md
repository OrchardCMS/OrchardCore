# Data Sources (`OrchardCore.DataPipelines.Core`)

The **Data Pipelines Core** feature (`OrchardCore.DataPipelines.Core`) of the [Data Pipelines](../DataPipelines/README.md) module is the data layer that data pipelines and other features, such as reports, share. It provides:

- the **data sources**, which expose the data of a site, such as its content items, its users and the results of its saved queries, as typed, tabular data sets. Features read data through them without knowing where the data is stored or who may see it;
- the **file formats** that rows are written to and read from: CSV, Excel, JSON and JSON Lines;
- the **formula language**, whose functions modules can add to;
- the **data operations** that sort, deduplicate, group and join any number of rows with a bounded amount of memory.

The feature has no admin screen of its own. It is enabled only as a dependency of the features that use it, such as the Data Pipelines feature (`OrchardCore.DataPipelines`). A module that reads data sources, or adds its own, depends on the `OrchardCore.DataPipelines.Core` feature, whose identifier is also the `DataPipelinesConstants.CoreFeatureId` constant:

```csharp
[assembly: Module(
    Name = "Support Reports",
    Author = "The Support Team",
    Version = "1.0.0",
    Dependencies =
    [
        "OrchardCore.DataPipelines.Core",
    ]
)]
```

## Concepts

| Concept | Description |
|---------|-------------|
| Data source | A connector that data is read from, such as the content items of the site. A data source is an `IDataSource` service with a stable technical name, such as `Contents`. |
| Data set | One table a data source exposes, such as the content items of the `Article` content type, or the users of the site. A data set has a stable technical name, a label, an optional group and an optional description. |
| Schema | The fields of a data set, in order. Each field has a stable technical name (which may contain dots, such as `TitlePart.Title`), a label, a type, and an optional group. |
| Field type | The type of the values of a field: `Text`, `Integer`, `Decimal`, `Boolean`, `Date` (a calendar date, with no time and no time zone) or `DateTime` (a date and a time: in UTC, as Orchard Core stores dates, or a local time without a time zone, such as a date and time converted to a time zone, or read from a file without an offset). |
| Batch | Rows are read one batch at a time (500 rows by default), so a consumer can read any number of rows without holding them all in memory. |

Each field type has one CLR type: `string` for `Text`, `long` for `Integer`, `decimal` for `Decimal`, `bool` for `Boolean`, and `DateTime` for `Date` and `DateTime`. A missing value is `null`.

A data source can also tell which fields identify a record (such as a content item id) and which data sets the values of a field refer to (such as the `Owner` of a content item, which refers to the `UserId` of the `Users` data set), so consumers can suggest related data sets and join them.

### Reading with the access of a user

Data is always read for a user, the `User` of the `DataSourceContext`. A data source lists only the data sets this user may read, returns no schema for a data set the user may not read, and checks the user's access again when it reads rows. While a pipeline is designed, data is read for the designer; when a published pipeline runs in the background, it is read for the user who published it.

## Built-in data sources

The built-in data sources are part of the Data Pipelines Core feature, and each one is available when the feature that owns its data is enabled too.

### Content items

Name: `Contents`. Requires the `OrchardCore.Contents` feature.

Each content type is a data set, named after the content type and grouped by its stereotype. A user sees the content types they have the `ViewContent` permission for. A user who also has the `EditContent` permission for a content type reads the latest version of its items, drafts included; any other user reads the published versions only.

Every content type has these fields:

| Field | Type | Description |
|-------|------|-------------|
| `ContentItemId` | Text | The content item id. |
| `ContentItemVersionId` | Text | The id of the version that is read. |
| `ContentType` | Text | The content type. |
| `DisplayText` | Text | The display text. |
| `Owner` | Text | The id of the user who owns the item. It refers to the `UserId` of the `Users` data set. |
| `Author` | Text | The name of the user who last changed the item. |
| `Published` | Boolean | Whether the version is published. |
| `Latest` | Boolean | Whether the version is the latest one. |
| `CreatedUtc` | DateTime | When the item was created. |
| `ModifiedUtc` | DateTime | When the item was last changed. |
| `PublishedUtc` | DateTime | When the item was published. |

They are followed, part by part, by the values of each part of the content type, and of its content fields:

- a value of a part becomes the field `{PartName}.{Property}`, such as `TitlePart.Title`, and a value of an object the part holds becomes `{PartName}.{Object}.{Property}`;
- a content field with one value becomes the field `{PartName}.{FieldName}`, such as `Product.Price`;
- a content field with several values becomes one field per value, `{PartName}.{FieldName}.{Property}`, such as `Product.Website.Url` and `Product.Website.Text`.

A value that holds a list, such as the content item ids of a content picker, is read as one text with the items separated by commas.

The values come from:

- **the values described in `ContentDataSourceOptions`**, which come first, with the types they are described with. The ones described by default are listed below;
- **the classes of the parts and content fields written in code**, registered with `AddContentPart<TPart>()` and `AddContentField<TField>()`: each other public property of the class is a value, typed by the type of the property, a number, a boolean, a date, a text, an enumeration, or a list of them. The properties of the objects a part holds are read too, up to three levels deep, and a content field that a part class declares as a property, such as the images of the `SeoMetaPart`, is read like the fields of the part's definition. Lists of objects, dictionaries, raw JSON, the properties marked `[JsonIgnore]`, and the properties whose name names a secret, such as `Password`, `Secret`, `ApiKey` or `ConnectionString`, or ends with `Token`, are left out;
- **the definition of each part**: the content fields added to a part in the admin are read, including the fields of the parts created in the admin, such as the part named after its content type.

A field type described in `ContentDataSourceOptions` is read only as described, so the fields named after it don't change; a part gets its described values, then the other properties of its class.

The parts and content fields described by default are:

| Part or field | Values |
|---------------|--------|
| `TitlePart` | `Title` |
| `AutoroutePart` | `Path` |
| `AliasPart` | `Alias` |
| `HtmlBodyPart` | `Html` |
| `MarkdownBodyPart` | `Markdown` |
| `ContainedPart` | `ListContentItemId`, `Order` |
| `LocalizationPart` | `Culture`, `LocalizationSet` |
| `PublishLaterPart` | `ScheduledPublishUtc` |
| `ArchiveLaterPart` | `ScheduledArchiveUtc` |
| `TextField` | `Text` |
| `NumericField` | `Value` (decimal) |
| `BooleanField` | `Value` |
| `DateField` | `Value` (date) |
| `DateTimeField` | `Value` (date and time) |
| `TimeField` | `Value` (text) |
| `HtmlField` | `Html` |
| `MarkdownField` | `Markdown` |
| `LinkField` | `Url`, `Text` |
| `ContentPickerField` | `ContentItemIds` (list) |
| `UserPickerField` | `UserIds` (list) |
| `TaxonomyField` | `TermContentItemIds` (list) |
| `MediaField` | `Paths` (list) |
| `MultiTextField` | `Values` (list) |
| `YoutubeField` | `RawAddress` |
| `LocalizationSetContentPickerField` | `LocalizationSets` (list) |

To give a value another name or type, or to read a value that no class describes, describe it, see [Reading more parts and fields](#reading-more-parts-and-fields).

The data source applies some conditions while it queries the database, such as a list of content item ids or owners, and ranges of the `CreatedUtc`, `ModifiedUtc` and `PublishedUtc` dates. It reads the items in pages, and releases each page from the session before reading the next one.

### Users

Name: `Users`. Requires the `OrchardCore.Users` feature.

The data sets are listed only for users who have the `ViewUsers` permission. Passwords, security stamps and other secrets of the users are never exposed.

| Data set | Description | Fields |
|----------|-------------|--------|
| `Users` | One row per user. | `UserId`, `UserName`, `Email`, `EmailConfirmed`, `PhoneNumber`, `PhoneNumberConfirmed`, `IsEnabled`, `TwoFactorEnabled`, `IsLockoutEnabled`, `LockoutEndUtc`, `AccessFailedCount`, and `Roles`, the role names separated by commas. |
| `UserRoles` | One row per role of each user. | `UserId`, `UserName`, `RoleName`. |

### Queries

Name: `Queries`. Requires the `OrchardCore.Queries` feature.

Each saved query is a data set, grouped by its source (such as `Sql` or `Lucene`). A user sees the queries they may execute, that is, the queries they have the `ExecuteApi_{QueryName}` permission for (implied by `ExecuteApiAll` and `ManageQueries`).

- A query that returns content items has the common fields of content items listed above, without the values of their parts and fields.
- The fields of any other query are inferred from a sample of its results, executed without parameters.

The parameters of a query are passed with `DataSourceQuery.Parameters`. In a data pipeline, they are entered in the **Parameters** setting of the **Read a data source** step, one `name=value` per line.

!!! note
    A query is executed as a whole, then its results are split into batches, so the number of rows a query returns is limited by the memory of the server. Use the query itself to limit its results.

## Filter operators

A consumer can offer conditions to a data source, with the operators of `DataFilterOperator`. Applying a condition is optional for a data source: consumers apply every condition again to the rows they read, so a data source may ignore the conditions it can't translate into a query of its store.

| Operator | Matches a value that |
|----------|----------------------|
| `Equals`, `NotEquals` | Equals, or differs from, the first value. |
| `Contains`, `NotContains` | Contains, or doesn't contain, the first value, ignoring case. |
| `StartsWith`, `EndsWith` | Starts or ends with the first value, ignoring case. |
| `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual` | Compares with the first value. |
| `Between` | Lies between the first and the second values, inclusive. An empty bound is open. |
| `In`, `NotIn` | Equals any, or none, of the values. |
| `IsEmpty`, `IsNotEmpty` | Is missing or an empty text, or isn't. |
| `InLastDays`, `InNextDays` | Is a date within the last, or next, N days, counting today, where N is the first value. |

## File formats

The feature provides file formats that rows can be written to and read from. They are `IDataFileFormat` services, listed by the `IDataFileFormatManager` service, which also finds the format of a file from its extension. The name, the label, the extension and the media type of a format are static members of its class, so the manager lists the formats with their `DataFileFormatDescriptor` without creating them, and `GetFormat(name)` returns the format to read or write a file with.

| Name | Format | Extension | Notes |
|------|--------|-----------|-------|
| `csv` | CSV (comma separated values) | `.csv` | Values that hold the delimiter, a quote or a line break are quoted, and quotes are doubled (RFC 4180). Files are written in UTF-8 with a byte order mark, so spreadsheet applications detect the encoding. The delimiter is a comma by default; `tab` or `\t` stands for a tab. Every column read from a CSV file is text. |
| `xlsx` | Excel workbook | `.xlsx` | Rows are written and read one at a time, so a large workbook never needs to fit in memory: each worksheet is streamed into the file as its rows arrive, and writing uses about 16 MB of memory whatever the number of rows. A workbook can hold several data sets, each in its own worksheet. A worksheet holds at most 1,048,576 rows, the row of column names included, which is the limit of Excel: writing more fails, with a message that suggests a CSV file or several worksheets. Numbers, booleans, dates and date-times keep their types. When reading, the type of each column is inferred from the values of the first batch. A worksheet is named `Sheet1` unless another name is given, and when reading, the first worksheet is read unless another one is named. |
| `json` | JSON | `.json` | An array with one object per row. When reading, the fields and their types are inferred from the objects of the first batch; properties that first appear later are ignored, and nested objects and arrays are read as JSON text. |
| `jsonl` | JSON Lines | `.jsonl` | One JSON object per line, which suits large exports. Fields are inferred as for JSON. |

When written to a text file, dates are formatted as `yyyy-MM-dd`, date-times in ISO 8601, ending with `Z` when they are in UTC and as they are when they are local times, numbers without grouping, and booleans as `true` or `false`.

The `DataFileOptions` class holds the options of a format: the `Delimiter` of a CSV file, whether the first row holds the column names (`HasHeaderRow`), the `SheetName` of a workbook, whether JSON is `Indented`, whether columns are named with the labels of the fields (`UseDisplayNames`), and the `BatchSize` of reading.

A format whose files hold several data sets, such as the Excel workbook, also implements `IDataWorkbookFormat`, from the `OrchardCore.DataSources.Files` namespace. Its `WriteSheetsAsync` method writes a list of `DataSheet` objects, each with a name, fields and batches of rows, to the worksheets of one workbook, and returns the number of rows written to each one; the `SheetName` of the `DataFileOptions` is ignored. The worksheets are written in order, and the batches of a sheet are read completely before those of the next sheet are started, so the batches of a later sheet must be able to wait, rather than come from a reader that is still in use. Worksheet names are made valid for Excel: the characters `: \ / ? * [ ]` are replaced with `_`, names are cut to 31 characters, an empty name becomes `SheetN`, where N is the position of the sheet, and a repeated name, ignoring case, gets a number such as ` (2)` appended. The **Create a file** step of data pipelines uses it to write each of its connections to its own worksheet.

```csharp
if (dataFileFormatManager.GetFormat("xlsx") is IDataWorkbookFormat workbookFormat)
{
    IReadOnlyList<long> rowCounts = await workbookFormat.WriteSheetsAsync(
        stream,
        [
            new DataSheet("Orders", orderFields, orderBatches),
            new DataSheet("Customers", customerFields, customerBatches),
        ],
        new DataFileOptions { UseDisplayNames = true });
}
```

## Formulas

The feature includes a formula language, used for example by the calculated fields and the filters of data pipelines. A formula computes a value from the fields of a row, such as `[Price] * [Quantity]` or `UPPER(TRIM([Country]))`.

### Syntax

| Element | Syntax |
|---------|--------|
| Field | The technical name of a field in square brackets, such as `[TitlePart.Title]`. A `]` in a field name is written `]]`, such as `[Size [cm]]]` for the field `Size [cm]`. |
| Text | In single or double quotes, such as `'CA'` or `"CA"`. A quote is escaped by doubling it, such as `'It''s'`. |
| Number | Digits, with a dot as the decimal separator, such as `42` or `0.25`. A number without a dot is an integer. |
| Constants | `TRUE`, `FALSE` and `NULL`. |
| Function | A name followed by its arguments in parentheses, such as `ROUND([Amount], 2)`. |
| Parentheses | Group a part of a formula, such as `([Price] + [Shipping]) * 1.15`. |

Keywords and function names are case-insensitive; field names are not.

### Operators

From the lowest precedence to the highest:

1. `OR` or `||`: true when either side is true.
2. `AND` or `&&`: true when both sides are true.
3. `NOT` or `!`: true when the condition is not true.
4. Comparisons: `=` or `==`, `!=` or `<>`, `<`, `<=`, `>`, `>=`.
5. `&`: joins two values as text. A missing value is joined as an empty text.
6. `+` and `-`: addition and subtraction.
7. `*`, `/` and `%`: multiplication, division and remainder.
8. Unary `-` and `+`: negation.

The operators follow these rules:

- A missing value in arithmetic gives a missing result, and so does a division by zero.
- Integer arithmetic that overflows continues with decimals.
- `+` joins text when one of its sides is text.
- Adding a number to a date, or subtracting it, adds or removes that number of days. Subtracting a date from another gives the number of days between them.
- A missing value equals another missing value or an empty text. `<`, `<=`, `>` and `>=` are false when either side is missing.
- A date compared with a text converts the text to a date, such as `[CreatedUtc] >= '2025-01-01'`.
- A value counts as true when it is `TRUE`, a number other than zero, or a text other than empty, `false` or `0`.

Functions return a missing value, rather than failing, when an argument is missing or of the wrong type. A formula is checked before it is evaluated: every field must exist, every function must be called with the right number of arguments, and operators must suit the types of their operands.

### Functions

These are the built-in functions. Modules can add functions, or replace built-in ones, see [Adding a formula function](#adding-a-formula-function).

#### Text

| Function | Description |
|----------|-------------|
| `UPPER(text)` | Converts text to upper case. |
| `LOWER(text)` | Converts text to lower case. |
| `TRIM(text)` | Removes leading and trailing spaces. |
| `PROPER(text)` | Capitalizes the first letter of each word. |
| `LEN(text)` | Returns the number of characters in the text. |
| `LEFT(text, count)` | Returns the first characters of the text. |
| `RIGHT(text, count)` | Returns the last characters of the text. |
| `MID(text, start, [count])` | Returns characters from the middle of the text. The first character is at position 1. |
| `SUBSTRING(text, start, [count])` | Same as `MID`. |
| `REPLACE(text, find, replacement)` | Replaces every occurrence of a text, ignoring case. |
| `CONCAT(value1, value2, ...)` | Joins values into one text. Missing values are skipped. |
| `FIND(text, search)` | Returns the position of the search text (starting at 1), or 0 when it is not found. |
| `CONTAINS(text, search)` | Returns true when the text contains the search text, ignoring case. |
| `STARTSWITH(text, search)` | Returns true when the text starts with the search text, ignoring case. |
| `ENDSWITH(text, search)` | Returns true when the text ends with the search text, ignoring case. |
| `SPLIT(text, separator, index)` | Splits the text and returns the part at the index (starting at 1). |
| `TEXT(value, [format])` | Converts a value to text, optionally with a .NET format such as `'N2'` or `'yyyy-MM'`. |

#### Number

| Function | Description |
|----------|-------------|
| `ABS(number)` | Returns the absolute value of a number. |
| `ROUND(number, [decimals])` | Rounds a number to a number of decimals (0 by default). Midpoints are rounded away from zero. |
| `FLOOR(number)` | Rounds a number down to a whole number. |
| `CEILING(number)` | Rounds a number up to a whole number. |
| `POWER(number, exponent)` | Raises a number to a power. |
| `SQRT(number)` | Returns the square root of a number. |
| `MOD(number, divisor)` | Returns the remainder of a division. |
| `SIGN(number)` | Returns -1, 0, or 1 for a negative, zero, or positive number. |
| `NUMBER(value)` | Converts a value to a decimal number. |
| `INTEGER(value)` | Converts a value to a whole number, dropping any fraction. |
| `LEAST(value1, value2, ...)` | Returns the smallest of the values. |
| `GREATEST(value1, value2, ...)` | Returns the largest of the values. |

#### Date

| Function | Description |
|----------|-------------|
| `NOW()` | Returns the current date and time. |
| `TODAY()` | Returns the current date. |
| `YEAR(date)` | Returns the year of a date. |
| `QUARTER(date)` | Returns the quarter of a date (1 to 4). |
| `MONTH(date)` | Returns the month of a date (1 to 12). |
| `DAY(date)` | Returns the day of the month of a date. |
| `WEEK(date)` | Returns the ISO week number of a date. |
| `WEEKDAY(date)` | Returns the day of the week of a date, from 1 (Monday) to 7 (Sunday). |
| `HOUR(date)` | Returns the hour of a date-time (0 to 23). |
| `MINUTE(date)` | Returns the minute of a date-time (0 to 59). |
| `DATE(year, month, day)`, `DATE(value)` | Builds a date from its parts, or converts a value to a date. |
| `DATETIME(value)` | Converts a value to a date-time. |
| `DATEADD('part', number, date)` | Adds a number of years, quarters, months, weeks, days, hours, minutes, or seconds to a date. |
| `DATEDIFF('part', start, end)` | Counts the years, quarters, months, weeks, days, hours, minutes, or seconds from start to end. |
| `DATETRUNC('part', date)` | Truncates a date to the start of its year, quarter, month, week, day, hour, or minute. |

The `part` of `DATEADD`, `DATEDIFF` and `DATETRUNC` is one of `year` (or `years`, `yyyy`, `yy`), `quarter` (`quarters`, `q`, `qq`), `month` (`months`, `mm`, `m`), `week` (`weeks`, `wk`, `ww`), `day` (`days`, `dd`, `d`), `hour` (`hours`, `hh`), `minute` (`minutes`, `mi`, `n`) and `second` (`seconds`, `ss`, `s`). Weeks start on Monday.

`NOW()` and `TODAY()` return the current date and time in the time zone of the site. In a data pipeline run, they return when the run started, so every row gets the same value.

#### Logical

| Function | Description |
|----------|-------------|
| `IF(condition, then, [else])` | Returns one value when the condition is true and another when it is not. |
| `IIF(condition, then, [else])` | Same as `IF`. |
| `IFS(condition1, value1, condition2, value2, ..., [else])` | Returns the value of the first condition that is true. |
| `SWITCH(value, match1, result1, match2, result2, ..., [else])` | Returns the result paired with the first match of the value. |
| `COALESCE(value1, value2, ...)` | Returns the first value that is not missing. |
| `IFNULL(value, fallback)` | Returns the fallback when the value is missing. |
| `ISNULL(value)` | Returns true when the value is missing. |
| `ISBLANK(value)` | Returns true when the value is missing or empty text. |
| `IN(value, option1, option2, ...)` | Returns true when the value equals any option. |
| `NOT(condition)` | Returns true when the condition is not true. |

#### Aggregate

Aggregate functions combine the values of a group of rows. They are available to consumers that evaluate formulas over groups of rows, and a formula can't mix aggregated values with the fields of a single row, or nest an aggregate function in another.

| Function | Description |
|----------|-------------|
| `SUM(number)` | Adds the values of the group. |
| `AVG(number)` | Averages the values of the group. |
| `AVERAGE(number)` | Same as `AVG`. |
| `MEDIAN(number)` | Returns the middle value of the group. |
| `MIN(value)` | Returns the smallest value of the group. |
| `MAX(value)` | Returns the largest value of the group. |
| `COUNTD(value)` | Counts the distinct values of the group. |
| `COUNT([value])` | Counts the rows of the group, or the rows where the value is present. |

!!! note
    The formulas of data pipeline steps read one row at a time, so they don't accept aggregate functions. Use the **Group and summarize** step to compute counts, sums and averages.

### Using formulas from code

The `ExpressionCompiler` class compiles a formula against a scope, which resolves the fields it refers to, and a catalog of functions. `FieldListExpressionScope` resolves the fields of a list of `DataField`, by their technical names. The `IExpressionFunctionCatalog` singleton holds the built-in functions and those the modules add:

```csharp
using OrchardCore.DataSources.Expressions;

// _functionCatalog is the IExpressionFunctionCatalog, injected in the constructor.
var scope = new FieldListExpressionScope(batch.Fields);

// Throws an ExpressionException, whose message is localized, when the formula is invalid.
var expression = ExpressionCompiler.Compile("[Price] * [Quantity]", scope, _functionCatalog, S);

foreach (var row in batch.Rows)
{
    var total = expression.Evaluate(new ExpressionContext { Row = row, Now = now });
}
```

`ExpressionCompiler.Compile(formula, scope, localizer)`, without a catalog, compiles a formula that calls the built-in functions only, those of `ExpressionFunctionCatalog.BuiltIn`, for code that runs without services.

`CompiledExpression.DataType` is the type of the values the formula returns, and `CompiledExpression.References` lists the fields it reads. `IExpressionFunctionCatalog.Functions` lists every function, ordered by category and name, with its signature and description, such as to show them to the people who write formulas, and `Find(name)` finds a function by name, ignoring case. The descriptions are in English; pass them through a localizer to show them.

## Data operations

Some operations must see many rows before they write any, such as a sort. The static classes of the `OrchardCore.DataSources.Operations` namespace, from the `OrchardCore.DataSources.Core` package, perform them on any number of rows with a bounded amount of memory: they keep the rows in memory up to a limit, write the rows that don't fit to temporary files, and delete these files when they are done. The **Sort rows**, **Join**, **Group and summarize** and **Remove duplicates** steps of data pipelines use them, and other modules, such as reports, can too.

A `DataSpillOptions` gives an operation the directory of its temporary files, created when needed, and the memory it may use, 32 MB by default (`DataSpillOptions.DefaultMemoryLimit`). The operations read rows as an `IAsyncEnumerable<object[]>` and return the rows they produce the same way:

| Operation | Description |
|-----------|-------------|
| `DataSorter.SortAsync` | Sorts rows with an external merge sort: rows are sorted in memory, written to temporary files as sorted runs when there are more than the limit allows, and the runs are merged while the rows are read. The sort is stable: rows that compare equal keep their order. `DataSorter.CreateComparer` creates the comparer of rows by fields, each ascending or descending: numbers compare by value, text ignoring case, and missing values come first. |
| `DataDistinct.DistinctAsync` | Keeps the first row of each set of rows with equal keys, and keeps the order of the rows. The keys that don't fit in memory have their rows deduplicated from a temporary file, once the others are done. |
| `DataAggregator.AggregateAsync` | Groups rows by the values of fields and computes a `DataMeasure` for each group, such as `new DataMeasure(DataAggregate.Sum, amountIndex)`, or `new DataMeasure(DataAggregate.Count, -1)` to count the rows. The groups come out in the order they first appear. Without group fields, every row belongs to one group, and an empty input still has one. The values of a median and of a distinct count stay in memory with their group. |
| `DataJoin.JoinAsync` | Joins two inputs on key fields, like a SQL join, with a `DataJoinType`: `Inner`, `Left`, `Right` or `Full`. The right input is read first: when it fits in memory, it becomes a lookup the left rows stream through (a hash join); otherwise both inputs are sorted by key in temporary files and merged. Either way, the rows come out in the same order: each left row with its matches in the order of the right input, then the right rows without a match. A row whose key has a missing value never matches. |

Grouping, deduplicating and joining build their keys with `DataKeys`: numbers compare by value (`2` equals `2.0`), text compares exactly unless asked to ignore case, and two missing values are equal.

```csharp
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

// _tempDirectoryProvider is the ITempDirectoryProvider, injected in the constructor.
var options = new DataSpillOptions(_tempDirectoryProvider.CreateTempSubdirectory("orders"), memoryLimit: 64L * 1024 * 1024);

// The largest orders first.
var comparer = DataSorter.CreateComparer([(fields.IndexOf("Amount"), Descending: true)]);

await foreach (var row in DataSorter.SortAsync(rows, comparer, options, cancellationToken))
{
    // The first row is available once every row was read.
}
```

Other classes of the namespace help write such operations:

- `DataRowBuffer` holds rows to read them later, any number of times, in the order they were added. The rows stay in memory until they exceed the limit, then they all go to a temporary file, deleted when the buffer is disposed.
- `DataRowQueue` is a first-in, first-out queue of batches of rows kept in a temporary file, for a producer that must not wait for a slower consumer.
- `DataFieldTransforms.Apply` changes the values of a field before they are grouped or shown, with a `DataFieldTransform`, such as the `Year` or the `Quarter` of a date, the first day of its `Month`, the `Day` of a date-time without its time, text in `Upper` case, or a number rounded with `Round`. A value the transform doesn't apply to becomes missing. `Supports` tells whether a transform applies to a field type, and `GetResultType` gives the type it returns.

## Extending data sources

### Define your own data source

The simplest way to add a data source is to derive from `RecordDataSource`, from the `OrchardCore.DataSources.Abstractions` package, and to describe each of its data sets as a `RecordDataSet<TRecord>` over a .NET type, such as a document or the row of an index:

- `AddField(name, displayName, type, read, group)` declares a field, in order, with the function that reads its value from a record. It returns the `DataField`, to set its other properties, such as `IsIdentifier`, `References`, or `IsKeyFilterable` when the data set loads only the records of a list of keys.
- `CanReadAsync` tells whether the user of the context may read the data set; everyone may by default. The data source lists, describes and reads only the data sets the user may read.
- `LoadAsync` streams the records, in a stable order, a page at a time, so that any number of records can be read. It stops when the enumeration stops: the base class reads only the fields and the number of rows the query asks for, and splits the rows into batches.

The values are converted to the type of their field: an enumeration value becomes its name, and a date becomes a UTC date (a `DateTimeOffset`, a local date or a date without a kind).

```csharp
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.Localization;

public sealed record Ticket(string TicketId, TicketState State, DateTimeOffset OpenedUtc, int Priority, string OwnerId);

public sealed class SupportDataSource : RecordDataSource, IDataSource
{
    public SupportDataSource(
        ITicketStore ticketStore,
        IAuthorizationService authorizationService,
        IStringLocalizer<SupportDataSource> localizer)
    {
        DataSets = [new TicketsDataSet(ticketStore, authorizationService, localizer)];
    }

    public static string Name => "Support";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SupportDataSource>("Support");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<SupportDataSource>("The tickets of the help desk.");

    protected override IEnumerable<IRecordDataSet> DataSets { get; }
}

public sealed class TicketsDataSet : RecordDataSet<Ticket>
{
    private const int PageSize = 500;

    private readonly ITicketStore _ticketStore;
    private readonly IAuthorizationService _authorizationService;

    public TicketsDataSet(ITicketStore ticketStore, IAuthorizationService authorizationService, IStringLocalizer localizer)
        : base(new DataSetDescriptor("Tickets", localizer["Tickets"], localizer["One row per ticket."]))
    {
        _ticketStore = ticketStore;
        _authorizationService = authorizationService;

        // LoadAsync reads only the tickets of a list of ids when a consumer asks for them, such as a join.
        var ticketId = AddField("TicketId", localizer["Ticket"], DataFieldType.Text, ticket => ticket.TicketId);
        ticketId.IsIdentifier = true;
        ticketId.IsKeyFilterable = true;

        // An enumeration value is read as its name, and a DateTimeOffset as a UTC date.
        AddField("State", localizer["State"], DataFieldType.Text, ticket => ticket.State);
        AddField("OpenedUtc", localizer["Opened"], DataFieldType.DateTime, ticket => ticket.OpenedUtc);
        AddField("Priority", localizer["Priority"], DataFieldType.Integer, ticket => ticket.Priority);

        // The owner refers to the users, so consumers can suggest to join them.
        var owner = AddField("OwnerId", localizer["Owner"], DataFieldType.Text, ticket => ticket.OwnerId);
        owner.IsIdentifier = true;
        owner.References.Add(new DataFieldReference("Users", "Users", "UserId"));
    }

    public override async Task<bool> CanReadAsync(DataSourceContext context, CancellationToken cancellationToken = default)
        => context.User is not null && await _authorizationService.AuthorizeAsync(context.User, SupportPermissions.ViewTickets);

    protected override async IAsyncEnumerable<Ticket> LoadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Apply the conditions the store understands; the consumer applies every condition again.
        var ticketIds = DataConditions.Keys(query.Conditions, "TicketId");
        var (openedFrom, openedTo) = DataConditions.DateRange(query.Conditions, "OpenedUtc");
        var skip = 0;

        while (true)
        {
            var tickets = await _ticketStore.GetTicketsAsync(ticketIds, openedFrom, openedTo, skip, PageSize, cancellationToken);

            if (tickets.Count == 0)
            {
                yield break;
            }

            foreach (var ticket in tickets)
            {
                yield return ticket;
            }

            skip += tickets.Count;
        }
    }
}
```

A data source implements `IDataSource`, whose `Name`, `DisplayName` and `Description` are static members of its class: `RecordDataSource` is a base class, so the data source declares `IDataSource` itself. `IDataSourceManager` lists the data sources with their `DataSourceDescriptor`, without creating them, and `GetDataSource(name)` resolves the data source to read from the services of the scope.

Register the data source with `AddDataSource<T>()`, from the `OrchardCore.DataSources.Core` package, in a module that depends on the `OrchardCore.DataPipelines.Core` feature, or in a startup that requires it:

```csharp
using OrchardCore.Modules;

[RequireFeatures("OrchardCore.DataPipelines.Core")]
public sealed class DataSourcesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<SupportDataSource>();
    }
}
```

The Data Pipelines Core feature is enabled only as a dependency of other features. With `[RequireFeatures]`, the data source is registered when a feature that depends on it, such as Data Pipelines, is enabled; with a dependency in the manifest of the module, enabling the module enables the Data Pipelines Core feature too.

`DataConditions` reads the conditions of a query that a data source can apply while it loads its records, such as in a database query:

- `DataConditions.Keys(conditions, field)` returns the values `In` and `Equals` conditions limit a field to, such as the keys of the rows a join needs, keeping only the values all of them allow, or `null` when no condition limits the field to a list of values. A field marked `IsKeyFilterable` must then be limited to these values exactly.
- `DataConditions.DateRange(conditions, field)` returns the narrowest range of dates, with inclusive bounds, that the comparison conditions of a field allow; a missing bound is `null`.

`DataField.WithName(name, displayName)` copies a field under another name, such as when a consumer prefixes the fields of a joined data set, keeping its type, group, description, whether it is an identifier, and the data sets it references.

### Confirming what was read

A data source that reads changes from a queue removes them only once they were delivered. `DataSourceQuery.Context` tells it who reads, and lets it ask to confirm:

- `ConsumerId` is a stable identifier of what reads, such as `{pipelineId}/{stepId}` for a step of a data pipeline, so the data source can keep the changes each consumer still has to read. It is `null` for a preview.
- `OnSucceeded(action)` adds an action to run once the consumer delivered what the data source read. A run of a data pipeline runs it only when the run succeeds, and only when its step read every row, so the changes of a failed run are read again; a preview never runs it.

```csharp
protected override async IAsyncEnumerable<ChangedOrder> LoadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var changes = await _queue.ListAsync(query.Context.ConsumerId, cancellationToken);

    foreach (var change in changes)
    {
        yield return await LoadOrderAsync(change, cancellationToken);
    }

    var ids = changes.Select(change => change.Id).ToArray();

    // Removed once the run delivered them.
    query.Context.OnSucceeded((services, token) => services.GetRequiredService<OrderChangeQueue>().RemoveAsync(ids, token));
}
```

### Adding a data source

A data source that doesn't read records one by one, such as one that lists its data sets dynamically or builds its batches itself, implements `IDataSource` directly, and is registered the same way, with `AddDataSource<T>()`. Its data sets are then available to every consumer, such as the **Read a data source** step of data pipelines.

```csharp
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.Localization;

public sealed class WarehouseDataSource : IDataSource
{
    private const string StockDataSet = "Stock";

    private readonly IWarehouseService _warehouseService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer S;

    public WarehouseDataSource(
        IWarehouseService warehouseService,
        IAuthorizationService authorizationService,
        IStringLocalizer<WarehouseDataSource> localizer)
    {
        _warehouseService = warehouseService;
        _authorizationService = authorizationService;
        S = localizer;
    }

    public static string Name => "Warehouse";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<WarehouseDataSource>("Warehouse");

    public static LocalizationSource Description { get; } = LocalizationSource.Create<WarehouseDataSource>("The stock of the warehouse.");

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
        => await CanReadAsync(context) ? [Describe()] : [];

    public async Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        // The schema is the security boundary: return null for a data set the user may not read.
        if (dataSet != StockDataSet || !await CanReadAsync(context))
        {
            return null;
        }

        return new DataSetSchema
        {
            DataSet = Describe(),
            Fields = [.. GetFields()],
        };
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Check the access of the user again: a data source yields nothing to a user who may not read the data set.
        if (query.DataSet != StockDataSet || !await CanReadAsync(query.Context))
        {
            yield break;
        }

        var fields = GetFields();
        var remaining = query.MaxRows > 0 ? query.MaxRows : int.MaxValue;
        var skip = 0;

        while (remaining > 0)
        {
            var take = Math.Min(Math.Max(1, query.BatchSize), remaining);
            var items = await _warehouseService.GetStockAsync(skip, take, cancellationToken);

            if (items.Count == 0)
            {
                yield break;
            }

            // Each row holds one value per field, in the order of the fields, with the CLR type of the field type.
            var rows = items
                .Select(item => new object[] { item.Sku, item.Name, (long)item.Quantity, item.UnitPrice })
                .ToList();

            yield return new DataBatch(fields, rows);

            skip += items.Count;
            remaining -= items.Count;
        }
    }

    private async Task<bool> CanReadAsync(DataSourceContext context)
        => context?.User is not null && await _authorizationService.AuthorizeAsync(context.User, WarehousePermissions.ViewStock);

    private DataSetDescriptor Describe()
        => new(StockDataSet, S["Stock"], S["One row per product."]);

    private DataField[] GetFields()
        =>
        [
            new("Sku", S["SKU"], DataFieldType.Text) { IsIdentifier = true },
            new("Name", S["Name"], DataFieldType.Text),
            new("Quantity", S["Quantity"], DataFieldType.Integer),
            new("UnitPrice", S["Unit price"], DataFieldType.Decimal),
        ];
}
```

```csharp
[RequireFeatures("OrchardCore.DataPipelines.Core")]
public sealed class DataSourcesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<WarehouseDataSource>();
    }
}
```

Data sources are scoped services. Two data sources can't be registered with the same name. Keep the technical names of data sets and fields stable: saved definitions, such as data pipelines, store them.

!!! tip
    Use `DataValues.Coerce(value, fieldType)` to convert a raw value to the CLR type of a field type, and `dataSource.ReadRowsAsync(query, maxRows)` to read the first rows of a data set into memory, such as for a preview.

### Adding a file format

Implement `IDataFileFormat` and register it with `AddDataFileFormat<T>()`. Formats are singletons. Their name, label, extension and media type are static, read once when the format is registered.

```csharp
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;
using OrchardCore.Localization;

public sealed class XmlDataFileFormat : IDataFileFormat
{
    public static string Name => "xml";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<XmlDataFileFormat>("XML");

    public static string Extension => ".xml";

    public static string ContentType => "application/xml";

    public Task<long> WriteAsync(
        Stream stream,
        IReadOnlyList<DataField> fields,
        IAsyncEnumerable<DataBatch> batches,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        // Write the rows as they arrive, and return the number of rows written.
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<DataBatch> ReadAsync(
        Stream stream,
        DataFileOptions options,
        CancellationToken cancellationToken = default)
    {
        // Yield the rows one batch at a time. A file with no rows yields one empty batch that carries its fields.
        throw new NotImplementedException();
    }
}
```

```csharp
services.AddDataFileFormat<XmlDataFileFormat>();
```

The format is then offered by the **Create a file** and **Read a media file** steps of data pipelines.

### Adding a formula function

Formulas call the functions of the `IExpressionFunctionCatalog`: the built-in functions, and those the modules register. Create a function with `ExpressionFunction.CreateScalar`, for a function that computes a value from the values of its arguments, one row at a time, or `ExpressionFunction.CreateAggregate`, for a function that combines the values of a group of rows, and register it with `services.AddExpressionFunction(function)`, from the `OrchardCore.DataSources.Core` package:

```csharp
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using OrchardCore.Modules;

[RequireFeatures("OrchardCore.DataPipelines.Core")]
public sealed class FormulasStartup : StartupBase
{
    private const decimal TaxRate = 0.15m;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddExpressionFunction(ExpressionFunction.CreateScalar(
            name: "TAX",
            category: ExpressionFunctions.NumberCategory,
            signature: "TAX(amount)",
            description: "Returns the sales tax of an amount.",
            minArguments: 1,
            maxArguments: 1,
            returnType: _ => DataFieldType.Decimal,
            // Return a missing value, rather than throwing, when an argument is missing or of the wrong type.
            invoke: args => args[0] switch
            {
                long amount => Math.Round(amount * TaxRate, 2),
                decimal amount => Math.Round(amount * TaxRate, 2),
                _ => null,
            }));
    }
}
```

The formula `[Price] + TAX([Price])` then adds the tax to a price, and the function is listed with the others, such as in the editors of the **Filter rows** and **Add calculated fields** steps of data pipelines.

- A function is called with the values of its arguments, with the CLR types of their field types: `long` for an integer, `decimal` for a decimal, and so on. `returnType` infers the type of the result from the types of the arguments.
- The name is case-insensitive. It starts with a letter or an underscore, holds only letters, digits and underscores, and can't be `AND`, `OR`, `NOT`, `TRUE`, `FALSE` or `NULL`. An invalid function fails when the catalog is first used.
- A registered function replaces a built-in function of the same name, whatever the order of the startups, and a function registered later replaces one of the same name registered before.
- The `category` groups the function in the list of functions, such as one of the `ExpressionFunctions` categories: `TextCategory`, `NumberCategory`, `DateCategory`, `LogicalCategory` or `AggregateCategory`, the category of the functions `CreateAggregate` creates.
- The `description` is in English: the list of functions passes it through the localizer.

An aggregate function combines the values of a group of rows, such as in the **Group and summarize** step. It takes one argument, and receives its values for every row of the group, with a `null` for a missing value. With `requiresNumericArgument`, the formula is invalid unless the argument is a number:

```csharp
services.AddExpressionFunction(ExpressionFunction.CreateAggregate(
    name: "RANGE",
    signature: "RANGE(number)",
    description: "Returns the difference between the largest and the smallest value of the group.",
    requiresNumericArgument: true,
    returnType: _ => DataFieldType.Decimal,
    aggregate: values =>
    {
        var numbers = values.Where(value => value is not null).Select(Convert.ToDecimal).ToList();

        // A group without values has no range.
        return numbers.Count == 0 ? null : numbers.Max() - numbers.Min();
    }));
```

To offer several functions, implement `IExpressionFunctionProvider`, whose `GetFunctions` returns them, and register it as a singleton with `services.AddExpressionFunctionProvider<TProvider>()`. The built-in functions come from the `BuiltInExpressionFunctionProvider`.

### Reading more parts and fields

The content items data source reads the parts and content fields registered in code from the properties of their classes, and the fields added in the admin from the definitions of the parts. A module describes the values of a part or a field type in `ContentDataSourceOptions`, from the `OrchardCore.DataPipelines` module, to read them differently: with another type, such as a number stored as text, as a list, or when the part or field has no class, such as a part a module stores without registering it. The described values show up as fields of the content type data sets, and can be written by the **Save content items** step of data pipelines, unless they are `IsReadOnly`. Each value is read from the JSON property of the same name, or from a nested property, with a path such as `Settings.Mode`:

```csharp
using OrchardCore.DataSources;
using OrchardCore.DataSources.Contents;

services.Configure<ContentDataSourceOptions>(options =>
{
    // A field type with one value becomes the field {PartName}.{FieldName}.
    options.Fields["ColorField"] = [new("Value", DataFieldType.Text)];

    // A field type with several values becomes one {PartName}.{FieldName}.{Property} field per value.
    options.Fields["AddressField"] =
    [
        new("Street", DataFieldType.Text),
        new("City", DataFieldType.Text),
    ];

    // A property that holds a list is read as text, its items separated by commas.
    options.Fields["TagsField"] = [new("Tags", DataFieldType.Text) { IsList = true }];

    // A part value becomes the field {PartName}.{Property}, labeled with its DisplayName.
    options.Parts["RatingPart"] =
    [
        new("Score", DataFieldType.Decimal),
        new("Votes.Count", DataFieldType.Integer) { DisplayName = "Votes", IsReadOnly = true },
    ];
});
```

`ContentDataValueDiscovery.Describe(type)` describes the values of a class as the data source reads them, which other modules can use to list the values of a part or a field type.
