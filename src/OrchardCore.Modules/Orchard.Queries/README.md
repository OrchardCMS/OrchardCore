# Queries (Orchard.Queries)

The queries module provide a management UI and APIs for querying data.

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

# SQL Queries (Orchard.Queries.Sql)

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