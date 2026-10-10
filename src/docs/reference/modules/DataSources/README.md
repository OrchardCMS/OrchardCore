# Data Sources (`OrchardCore.DataSources`)

The Data Sources module exposes the data of a site, such as its content items, its users and the results of its saved queries, as typed, tabular data sets. Other features read data through it without knowing where the data is stored or who may see it: [Data Pipelines](../DataPipelines/README.md) read data sources to transform and deliver data, and other modules, such as reporting tools, can reuse the same data sources, file formats and formula language.

The module has no admin screen of its own. It is enabled as a dependency of the features that use it.

## Concepts

| Concept | Description |
|---------|-------------|
| Data source | A connector that data is read from, such as the content items of the site. A data source is an `IDataSource` service with a stable technical name, such as `Contents`. |
| Data set | One table a data source exposes, such as the content items of the `Article` content type, or the users of the site. A data set has a stable technical name, a label, an optional group and an optional description. |
| Schema | The fields of a data set, in order. Each field has a stable technical name (which may contain dots, such as `TitlePart.Title`), a label, a type, and an optional group. |
| Field type | The type of the values of a field: `Text`, `Integer`, `Decimal`, `Boolean`, `Date` (a calendar date, with no time and no time zone) or `DateTime` (a point in time, in UTC). |
| Batch | Rows are read one batch at a time (500 rows by default), so a consumer can read any number of rows without holding them all in memory. |

Each field type has one CLR type: `string` for `Text`, `long` for `Integer`, `decimal` for `Decimal`, `bool` for `Boolean`, and `DateTime` for `Date` and `DateTime`. A missing value is `null`.

A data source can also tell which fields identify a record (such as a content item id) and which data sets the values of a field refer to (such as the `Owner` of a content item, which refers to the `UserId` of the `Users` data set), so consumers can suggest related data sets and join them.

### Reading with the access of a user

Data is always read for a user, the `User` of the `DataSourceContext`. A data source lists only the data sets this user may read, returns no schema for a data set the user may not read, and checks the user's access again when it reads rows. While a pipeline is designed, data is read for the designer; when a published pipeline runs in the background, it is read for the user who published it.

## Built-in data sources

The built-in data sources are available when the feature that owns their data is enabled.

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

They are followed by the values of the parts and fields of the content type that `ContentDataSourceOptions` describes:

- a value of a part becomes the field `{PartName}.{Property}`, such as `TitlePart.Title`;
- a content field with one value becomes the field `{PartName}.{FieldName}`, such as `Product.Price`;
- a content field with several values becomes one field per value, `{PartName}.{FieldName}.{Property}`, such as `Product.Website.Url` and `Product.Website.Text`.

A value that holds a list, such as the content item ids of a content picker, is read as one text with the items separated by commas.

The parts and content fields mapped by default are:

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

Other parts and fields are not read until they are described, see [Reading more parts and fields](#reading-more-parts-and-fields).

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

The module provides file formats that rows can be written to and read from. They are `IDataFileFormat` services, listed by the `IDataFileFormatManager` service, which also finds the format of a file from its extension.

| Name | Format | Extension | Notes |
|------|--------|-----------|-------|
| `csv` | CSV (comma separated values) | `.csv` | Values that hold the delimiter, a quote or a line break are quoted, and quotes are doubled (RFC 4180). Files are written in UTF-8 with a byte order mark, so spreadsheet applications detect the encoding. The delimiter is a comma by default; `tab` or `\t` stands for a tab. Every column read from a CSV file is text. |
| `xlsx` | Excel workbook | `.xlsx` | Rows are written and read one at a time, so a large workbook never needs to fit in memory. Numbers, booleans, dates and date-times keep their types. When reading, the type of each column is inferred from the values of the first batch. A worksheet is named `Sheet1` unless another name is given, and when reading, the first worksheet is read unless another one is named. |
| `json` | JSON | `.json` | An array with one object per row. When reading, the fields and their types are inferred from the objects of the first batch; properties that first appear later are ignored, and nested objects and arrays are read as JSON text. |
| `jsonl` | JSON Lines | `.jsonl` | One JSON object per line, which suits large exports. Fields are inferred as for JSON. |

When written to a text file, dates are formatted as `yyyy-MM-dd`, date-times in ISO 8601 UTC, numbers without grouping, and booleans as `true` or `false`.

The `DataFileOptions` class holds the options of a format: the `Delimiter` of a CSV file, whether the first row holds the column names (`HasHeaderRow`), the `SheetName` of a workbook, whether JSON is `Indented`, whether columns are named with the labels of the fields (`UseDisplayNames`), and the `BatchSize` of reading.

## Formulas

The module includes a formula language, used for example by the calculated fields and the filters of data pipelines. A formula computes a value from the fields of a row, such as `[Price] * [Quantity]` or `UPPER(TRIM([Country]))`.

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

The `ExpressionCompiler` class compiles a formula against a scope, which resolves the fields it refers to. `FieldListExpressionScope` resolves the fields of a list of `DataField`, by their technical names:

```csharp
using OrchardCore.DataSources.Expressions;

var scope = new FieldListExpressionScope(batch.Fields);

// Throws an ExpressionException, whose message is localized, when the formula is invalid.
var expression = ExpressionCompiler.Compile("[Price] * [Quantity]", scope, S);

foreach (var row in batch.Rows)
{
    var total = expression.Evaluate(new ExpressionContext { Row = row, Now = now });
}
```

`CompiledExpression.DataType` is the type of the values the formula returns, and `CompiledExpression.References` lists the fields it reads. `ExpressionFunctions.All` lists every function, with its category, signature and description.

## Extending data sources

### Adding a data source

Implement `IDataSource` and register it with `AddDataSource<T>()`, from the `OrchardCore.DataSources.Core` package. Its data sets are then available to every consumer, such as the **Read a data source** step of data pipelines.

```csharp
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;

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

    public string Name => "Warehouse";

    public LocalizedString DisplayName => S["Warehouse"];

    public LocalizedString Description => S["The stock of the warehouse."];

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
public sealed class Startup : StartupBase
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

Implement `IDataFileFormat` and register it with `AddDataFileFormat<T>()`. Formats are singletons.

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Files;

public sealed class XmlDataFileFormat : IDataFileFormat
{
    public XmlDataFileFormat(IStringLocalizer<XmlDataFileFormat> localizer)
    {
        DisplayName = localizer["XML"];
    }

    public string Name => "xml";

    public LocalizedString DisplayName { get; }

    public string Extension => ".xml";

    public string ContentType => "application/xml";

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

### Reading more parts and fields

The content items data source reads the parts and content fields that `ContentDataSourceOptions` describes. A module that adds a part or a field type describes its values, so they show up as fields of the content type data sets, and can be written by the **Save content items** step of data pipelines. Each value is read from the JSON property of the same name:

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

    // A part value becomes the field {PartName}.{Property}.
    options.Parts["RatingPart"] = [new("Score", DataFieldType.Decimal)];
});
```
