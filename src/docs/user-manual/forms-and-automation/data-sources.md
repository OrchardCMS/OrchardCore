---
title: Data sources
description: The site data that data pipelines can read (content items, users and saved queries), the fields each one offers, how to filter it, and the file formats and formulas that go with it.
technical_manual:
  - reference/modules/DataSources/README.md
  - reference/modules/DataPipelines/README.md
  - reference/modules/Queries/README.md
---

# Data sources

Data sources are what lets a [data pipeline](data-pipelines.md) read the data of the site as simple tables of rows and fields: the content items of each content type, the users and their roles, and the results of saved queries. Each data source offers one or more **data sets** (one table each), such as the **Blog Post** content type or the **Users** list. This page explains where you choose them, what each one reads, how to filter the rows, and the file formats and formulas that come with them.

| | |
| --- | --- |
| **Menu** | None of its own: you meet data sources in **Tools > Data Pipelines > Pipelines**, in the designer |
| **Permission** | *Manage data pipelines* to add them to a pipeline, plus the permissions to see the data itself (see each data source below) |
| **Feature** | **Data Pipelines Core** (`OrchardCore.DataPipelines.Core`), turned on automatically with **Data Pipelines** (`OrchardCore.DataPipelines`) |

!!! note "No screen of its own"
    You don't turn **Data Pipelines Core** on yourself: it turns on by itself when a feature that needs it, such as **Data Pipelines**, is turned on, and **Tools > Features** shows it only then. It has no page for data sources. The only screen it adds is **Tools > Shared Links**, described in [Shared links](shared-links.md).

## Where you choose a data source

1. Open a pipeline in the designer (**Tools > Data Pipelines > Pipelines**, then **Edit**).
2. Click **Add a data source** in the toolbar. On an empty pipeline you can also click **Read a data source** under **Start with a source**, or drag **Read a data source** from the **Sources** of the toolbox.
3. The **Add a data source** dialog lists every data set you may read, as cards with their name, their data source and their group. On the left, pick **All** or one data source (**Content items**, **Users**, **Queries**), each with its number of data sets. Type in **Filter** to search by name or description.
4. Click **Add** on a data set.

The designer adds a **Read a data source** step for it. In that step's settings, **Change** next to **Data set** reopens the dialog; choosing another data set clears the fields and the filters of the step. The step's other settings (fields to read, filters, maximum number of rows, read only new rows, parameters) are described in [Data pipeline steps](data-pipeline-steps.md#sources).

The **Fields** tab of the step lists the fields of the data set with their label, their reference (such as `[DisplayText]`) and their type.

## Who sees which data

Data is always read for a person:

- In the designer, the dialog lists only the data sets **you** may read, and previews read with your access.
- When a published pipeline runs, it reads with the access of the person **who published it**. A data set that person can't read is reported as missing.

So two people may see different lists in the **Add a data source** dialog. If a data set you expect is missing, check your role, or that the feature that owns the data is on.

## Content items

Data source **Content items**, available when **Contents** (`OrchardCore.Contents`) is on.

- **Data sets**: one per content type, such as **Article** or **Blog Post**. Each card shows the stereotype of the type as its group (for example **Widget** or **MenuItem**), or **Content** for ordinary types.
- **Who sees it**: you see a content type when your role may view its content. If your role may also edit content of that type, you read the latest version of each item, drafts included; otherwise you read published versions only.

Every content type has these fields:

| Field | Type | What it holds |
| --- | --- | --- |
| **Content item id** | Text | The id of the item. |
| **Version id** | Text | The id of the version read. |
| **Content type** | Text | The content type. |
| **Display text** | Text | The title shown in lists. |
| **Owner** | Text | The id of the user who owns the item (it matches **User id** in the users data set). |
| **Author** | Text | The name of the user who last changed it. |
| **Published** | Boolean | Whether the version is published. |
| **Latest** | Boolean | Whether it is the latest version. |
| **Created** | Date and time | When the item was created. |
| **Modified** | Date and time | When it was last changed. |
| **Published on** | Date and time | When it was published. |

They are followed by the parts and fields of the content type, named after them: for example **Title** (`TitlePart.Title`), **Autoroute Path** (`AutoroutePart.Path`) or a **Subtitle** field of the Blog Post type (`BlogPost.Subtitle`). Fields with several values, such as a link field, give one field per value (its URL and its text). Lists, such as the items of a content picker or the terms of a taxonomy field, are read as one text with the ids separated by commas.

The **Save content items** step writes to the same parts and fields; see [Data pipeline steps](data-pipeline-steps.md#destinations).

## Users

Data source **Users**, available when **Users** (`OrchardCore.Users`) is on. You see it when your role has *View user profiles*. Passwords and other secrets are never read.

| Data set | Rows | Fields |
| --- | --- | --- |
| **Users** | One row per user. | **User id**, **User name**, **Email**, **Email confirmed**, **Phone number**, **Phone number confirmed**, **Enabled**, **Two-factor authentication**, **Lockout enabled**, **Locked out until**, **Failed sign-ins**, **Roles** (the role names separated by commas). |
| **User roles** | One row per role of each user. | **User id**, **User name**, **Role**. |

To show the user names of content items, add a **Join** step with **Owner** on the left and **User id** on the right.

## Queries

Data source **Queries**, available when **Queries** (`OrchardCore.Queries`) is on. Each saved query is a data set, named after the query and grouped by its kind (such as **Sql** or **Lucene**). Its description says whether it returns content items. See [Queries](../search/queries.md) to create saved queries.

- **Who sees it**: you see a query when your role may run it, that is, has its *Execute Api - {query name}* permission, *Execute Api - All queries* or *Manage queries*.
- **Fields**: a query that returns content items has the common content item fields listed above (without the parts and fields). For any other query, the fields are worked out from a sample of its results.
- **Parameters**: when a query takes parameters, type them in the **Parameters** box of the **Read a data source** step, one `name=value` per line, for example `limit=10`.

!!! note
    A query runs as a whole before its rows are passed on, so a query that returns a very large number of rows uses a lot of server memory. Limit the results in the query itself.

## Field types

Every field has one of these types, shown on the **Fields** tab and offered by **Select fields**:

| Type | Values |
| --- | --- |
| **Text** | Any text. |
| **Integer** | Whole numbers. |
| **Decimal** | Numbers with decimals. |
| **Boolean** | Yes or no (true or false). |
| **Date** | A calendar date, without a time. |
| **Date and time** | A date and a time. Orchard Core stores them in UTC; use the **Convert time zone** step to show local times. |

## Filter the rows of a data source

In the **Read a data source** step, click **Add a filter**, then choose a **Field**, a **Comparison** and a value. Only the rows that match every filter are read. The editor offers only the comparisons that suit the type of the field:

| Field type | Comparisons |
| --- | --- |
| Text | **equals**, **doesn't equal**, **contains**, **doesn't contain**, **starts with**, **ends with**, **is one of**, **isn't one of**, **is empty**, **isn't empty** |
| Integer, Decimal | **equals**, **doesn't equal**, **is greater than**, **is greater than or equal to**, **is less than**, **is less than or equal to**, **is between**, **is one of**, **isn't one of**, **is empty**, **isn't empty** |
| Date, Date and time | The comparisons of numbers, plus **is in the last** and **is in the next** a number of **days**, counting today |
| Boolean | **equals**, **doesn't equal**, **is one of**, **isn't one of**, **is empty**, **isn't empty** |

- **is between** takes a value and a **Second value**; leave one empty for an open range.
- **is one of** and **isn't one of** take **Values, separated by commas**, such as `Paris, London`. Put a value that holds a comma in double quotes, such as `"Smith, John"`.
- Dates and times are compared in UTC.
- The text button next to the value (**Write the value as text**) lets you type a placeholder, such as `{Parameter:Since}` for a value a workflow passes, or a date without a time on a date and time field, which then covers the whole day.

## File formats

The same feature provides the file formats that the **Create a file** step writes and the **Read a media file** step reads:

| Format | Notes |
| --- | --- |
| **CSV (comma separated values) (.csv)** | Opens in any spreadsheet. The delimiter is a comma unless you change it (type `tab` for tabs). Every column read from a CSV file is text. |
| **Excel workbook (.xlsx)** | Keeps numbers, dates and yes/no values as such. A workbook can hold several worksheets. A worksheet holds at most 1,048,576 rows, the limit of Excel; for more, use CSV or several worksheets. |
| **JSON (.json)** | A list of objects, one per row. |
| **JSON Lines (.jsonl)** | One object per line, suited to very large exports. |

## Write formulas

The **Filter rows** and **Add calculated fields** steps use formulas, such as `[Price] * [Quantity]` or `UPPER(TRIM([Country]))`. Below the formula boxes, click a field to insert it, or open **Functions and operators** and search for a function to insert it.

| Element | How to write it |
| --- | --- |
| A field | Its reference in square brackets, as the **Fields** tab shows it: `[TitlePart.Title]`. Field names are case-sensitive. |
| Text | In quotes: `'CA'` or `"CA"`. |
| A number | With a dot for decimals: `42`, `0.25`. |
| Constants | `TRUE`, `FALSE`, `NULL`. |
| Comparisons | `=`, `!=` (or `<>`), `<`, `<=`, `>`, `>=`. |
| Arithmetic | `+`, `-`, `*`, `/`, `%`. |
| Join text | `&`, such as `[FirstName] & ' ' & [LastName]`. |
| Conditions | `AND`, `OR`, `NOT`. |

Some useful rules:

- A missing value in a calculation gives a missing result, and so does a division by zero.
- Adding a number to a date adds that many days; subtracting two dates gives the number of days between them.
- A date compared with text reads the text as a date: `[CreatedUtc] >= '2025-01-01'`.
- A function that gets a missing value, or a value of the wrong type, returns a missing value instead of failing.
- In **Add calculated fields**, a value that can't be calculated for a row is left empty and noted as a warning. In **Filter rows**, a row whose condition can't be evaluated doesn't match, and is noted as a warning.

Built-in functions, by category:

| Category | Functions |
| --- | --- |
| Text | `UPPER`, `LOWER`, `TRIM`, `PROPER`, `LEN`, `LEFT`, `RIGHT`, `MID`, `SUBSTRING`, `REPLACE`, `CONCAT`, `FIND`, `CONTAINS`, `STARTSWITH`, `ENDSWITH`, `SPLIT`, `TEXT` |
| Number | `ABS`, `ROUND`, `FLOOR`, `CEILING`, `POWER`, `SQRT`, `MOD`, `SIGN`, `NUMBER`, `INTEGER`, `LEAST`, `GREATEST` |
| Date | `NOW`, `TODAY`, `YEAR`, `QUARTER`, `MONTH`, `DAY`, `WEEK`, `WEEKDAY`, `HOUR`, `MINUTE`, `DATE`, `DATETIME`, `DATEADD`, `DATEDIFF`, `DATETRUNC` |
| Logical | `IF`, `IIF`, `IFS`, `SWITCH`, `COALESCE`, `IFNULL`, `ISNULL`, `ISBLANK`, `IN`, `NOT` |

The **Functions and operators** list in the editor shows what each function does and its arguments. `NOW()` and `TODAY()` use the site time zone, and give the same value to every row of a run. Formulas work one row at a time, so totals such as sums and counts are made with the **Group and summarize** step instead.

## More data sources

Developers can add data sources of their own, such as the records of a custom module or an outside service, and other features (for example reports from other modules) can read the same data sources. They then appear in the **Add a data source** dialog like the built-in ones. Your developer sets this up; see the Technical Manual.
