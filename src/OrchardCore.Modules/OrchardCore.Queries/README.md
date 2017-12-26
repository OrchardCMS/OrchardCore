# Queries (OrchardCore.Queries)

The queries module provides a management UI and APIs for querying data.

## Creating custom query sources

### Query

Create a class inheriting from `Query` which will represent the state that is necessary to represent this new query.

### QuerySource

Create a class implementing `IQuerySource` in order to expose the new type of query. The query source can be registered
like this:

```
services.AddScoped<IQuerySource, LuceneQuerySource>();
```

### Editors

Queries are edited by providing custom implementation of a `DisplayDriver` for the type `Query`. 

```
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

### api/queries/{name}

Executes a query with the specified name.

Verbs: **POST** and **GET**

| Parameter | Example | Description |
| --------- | ---- |------------ |
| `name` | `myQuery` | The name of the query to execute |
| `parameters` | `{ size: 3}` | A Json object representing the parameters of the query |

# SQL Queries (OrchardCore.Queries.Sql)

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

## query

The `query` filter provides a way to access named queries.
To access a named query, use the name as the input.


```
{% assign recentBlogPosts = "RecentBlogPosts" | query %}
{% for item in recentBlogPosts %}
{{ item | display_text }}
{% endfor %}
```

The example above will iterate over all the results of the query name `RecentBlogPosts` and display the text representing
the content item. Any available property on the results of the queries can be used. This example assumes the results
will be content items.

# Razor Helpers

The `QueryAsync` and `ContentQueryAsync` OrchardRazorHelper extension methods (in the `OrchardCore.Queries` and `OrchardCore.ContentManagement` namespaces respectively) allow you to run queries directly from razor pages.

You can use the `DisplayAsync` extension method (also in `OrchardCore.ContentManagement`) to display the content items returned from `ContentQueryAsync`.

For example, to run a query called LatestBlogPosts, and display the results:

```
@foreach (var contentItem in await OrchardCore.ContentQueryAsync("AllContent"))
{
    @await OrchardCore.DisplayAsync(contentItem)
}
```
