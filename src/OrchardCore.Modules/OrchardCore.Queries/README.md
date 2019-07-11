# Queries (`OrchardCore.Queries`)

The queries module provides a management UI and APIs for querying data.

## Creating custom query sources

### Query

Create a class inheriting from `Query` which will represent the state that is necessary to represent this new query.

### QuerySource

Create a class implementing `IQuerySource` in order to expose the new type of query.
The query source can be registered like this:

```csharp
services.AddScoped<IQuerySource, LuceneQuerySource>();
```

### Editors

Queries are edited by providing a custom implementation of a `DisplayDriver` for the type `Query`. 

```csharp
public class LuceneQueryDisplayDriver : DisplayDriver<Query, LuceneQuery>
{
...
}
```

### Queries dialog

When the list of query types is displayed, a template for the shape `Query_Link__[QuerySource]` will be used.
For instance, if the source is `Lucene` then the file `Query-Lucene.Link.cshtml` will be used.

## Recipe step

Queries can be created during recipes using the `queries` step.
Here is a sample step:

```json
{
    "name": "queries",
    "Queries": [ {
        // Common properties
        "Name": "AwesomeQuery",
        "Source": "Lucene",
        // Properties of the concrete query
        ...
        }
    ]
}

```

## Web APIs

### `api/queries/{name}`

Executes a query with the specified name.

Verbs: **POST** and **GET**

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `name` | `myQuery` | The name of the query to execute. |
| `parameters` | `{ size: 3}` | A Json object representing the parameters of the query. |

# SQL Queries (`OrchardCore.Queries.Sql`)

This feature provide a new type of query targeting the SQL database.

### Queries recipe step

Here is an example for creating a SQL query from a Queries recipe step:

```json
{
    "Source": "Sql",
    "Name": "ContentItems",
    "Template": "select * from ContentItemIndex", // json encoded query template
    "ReturnDocuments": false
}
```

# Liquid templates

You can access queries from liquid views and templates by using the `Queries` property. Queries are accessed by name, for example `Queries.RecentBlogPosts`.

## query

The `query` filter provides a way to execute queries. 

```liquid
{% assign recentBlogPosts = Queries.RecentBlogPosts | query %}
{% for item in recentBlogPosts %}
{{ item | display_text }}
{% endfor %}
```

The example above will iterate over all the results of the query name `RecentBlogPosts` and display the text representing the content item.
Any available property on the results of the queries can be used. This example assumes the results will be content items.

### Parameters

The `query` filter allows you to pass in parameters to your parameterized queries. For example, a query called `ContentItems` that has two parameters (`contentType` and `limit`) can be called like this:

```liquid
{% assign fiveBlogPosts = Queries.ContentItems | query: contentType: "BlogPost", limit: 5 %}
```

# Razor Helpers

The `QueryAsync` and `ContentQueryAsync` Orchard Helper extension methods (in the `OrchardCore.Queries` and `OrchardCore.ContentManagement` namespaces respectively) allow you to run queries directly from razor pages.

You can use the `DisplayAsync` extension method (also in `OrchardCore.ContentManagement`) to display the content items returned from `ContentQueryAsync`.

For example, to run a query called `LatestBlogPosts`, and display the results:

```liquid
@foreach (var contentItem in await OrchardCore.ContentQueryAsync("AllContent"))
{
    @await Orchard.DisplayAsync(contentItem)
}
```

> The Razor Helper is accessible on the `Orchard` property if the view is using Orchard Core's Razor base class, or by injecting `OrchardCore.IOrchardHelper` in all other cases.

# Executing SQL Queries

## RDBMS support
Because RDMBS vendors support different SQL flavors this module will analyze the query you defined and render a specific one based on the RDBMS that is used.
This also allows the queries to be exported and shared across website instances even if they run on different RDBMS.

## Examples

Here is an example of a query that returns all published Blog Posts:

```sql
    select DocumentId
    from ContentItemIndex 
    where Published = true and ContentType = 'BlogPost'
```

By selecting the "Return documents" options, the content items associated with the resulting `DocumentId` values are loaded.

The example below returns a custom set of values instead of content items:

```sql
select 
    month(CreatedUtc) as [Month], 
    year(CreatedUtc) as [Year],
    day(CreatedUtc) as [Day],
    count(*) as [Count]
from ContentItemIndex 
where Published = true and ContentType = 'BlogPost'
group by day(CreatedUtc), month(CreatedUtc), year(CreatedUtc)
```

## Parameters

Parameters can be provided when running queries.
Parameters are safe to use as they will always be parsed before being included in a query.
The syntax of a parameter is `@name:default_value`,
where `name` is the name of the parameter, and `default_value` an expression (usually a literal) to use in case
the parameter is not defined.

The following example loads the document ids for a parameterized content type:

```sql
select DocumentId
from ContentItemIndex 
where Published = true and ContentType = @contenttype:'BlogPost'
```

If the `contenttype` parameter is not passed when the query is invoked, then the default value is used.

Parameter names are case-sensitive.

## Templates

A SQL query is actually a Liquid template. This allows your queries to be shaped based on the parameters it gets. 
When injecting user-provided values, be sure to encode these such that they can't be exploited.
It is recommended to use parameters to inject values in the queries, and only use Liquid templates to change the shape of the query.

This example checks that a `limit` parameter is provided and if so uses it:

```liquid
{% if limit > 0 %}
    select ... limit @limit
{% else %}
    select ... 
{% endif %}
```

## Paging

Use `LIMIT [number]` and `OFFSET [number]` to define paged results.

These statements will be converted automatically based on the RDBMS in use.

## Helper functions

The SQL parser is also able to convert some specific functions to the intended dialect.

| Name             | Description                        |
| ---------------- |----------------------------------- |
| `second(_date_)` | Returns the seconds part of a date. |
| `minute(_date_)` | Returns the minutes part of a date. |
| `hour(_date_)`   | Returns the hours part of a date.   |
| `day(_date_)`    | Returns the days part of a date.    |
| `month(_date_)`  | Returns the months part of a date.  |
| `year(_date_)`   | Returns the years part of a date.   |

# Scripting

The following JavaScript functions are available with this module.

| Function | Description | Signature |
| -------- | ----------- | --------- |
| `executeQuery` | Returns the result of the query. | `executeQuery(name: String, parameters: Dictionary<string,object>): IEnumerable<object>` |