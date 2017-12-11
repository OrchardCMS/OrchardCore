# GraphQL API

## Query
Every GraphQL schema has a root type for both queries and mutations. The query type defines GraphQL operations that retrieve data from the server.

### contentItem
The following query looks up a content item on the contentItemId `A913LFSOAQWEM`, the query then returns the `contentItemId` and `contentType`

```graphql
query {
  contentItem(contentItemId:"A913LFSOAQWEM") {
    contentItemId
    contentType
  }
}
```

The schema at this point doesnt know about contentparts. This query is for just the raw content item.

### contentItems
Similar to the previous query, this query will return a list of content items, rather than a singular one.

This query will return all the `contentItemId` and `contentType` values of content with a content type of `BlogPost`.

```graphql
query {
  contentItems(contentType: "BlogPost") {
    contentItemId
    contentType
  }
}
```

### Dynamic Querying
Because Orchard has a very fluid content model, i.e. You can create content types on the fly, and these are not represented in code, content types are dynamically built.

An example is, lets say a `Blog` content type exists within the content type system, here you can query all blogs.

```graphql
query {
  blog(id: "D394KFSDERFDM") {
    contentItemId
    contentType
  }
}
```

This query will return a list of all blog's `contentItemId` and `contentType` properties. These endpoints do not have a singular representation.

#### Query, returning content part details
All content items contain content parts, and once you have a content item, you will more than likely want details of, lets say the title on the titlePart.

So lets return a Blog, with an Id of "D394KFSDERFDM", and display its Title contained on the TitlePart.

```graphql
query {
  blog(id: "D394KFSDERFDM") {
    contentItemId
    contentType
    titlePart {
        title
    }
  }
}
```

> Note: Only content parts that exist on content items will work

### Named Queries
Modules can define named queries. These queries are predefined by the modules themselves, and not specified in the schema.

You can define a query using the interface `INamedQueryProvider`. This allows you to return a list of named queries.

```c#
public class NamedQueryProvider : INamedQueryProvider {
  public IDictionary<string, string> Resolve() 
  {
    var queries = new Dictionary<string, string>();
 
    queries["allblogs"] = @"query blog { contentItemId }";

    return queries;
  }
}
```

You then pass up a `namedquery` parameter in the request to the endpoint.

## Mutations
Every GraphQL schema has a root type for both queries and mutations. The mutation type defines GraphQL operations that change data on the server. It is analogous to performing HTTP verbs such as `POST`, `PATCH`, and `DELETE`.

### createSite
Create a tenant

#### Input Fields

##### siteName (`!String`)
The name of the site

##### databaseProvider (`!String`)
The database the tenant will use, eg. Sqlite

##### userName (`!String`)
The user name of the site admin

##### email (`!String`)
The email address of the site admin

##### password (`!String`)
password to log in with

##### recipeName (`!String`)
The recipe to run to load the site, eg. `Blog` or `Agency`

The recipe names are defined in the recipe files marked `*.recipe.json`.

#### Return Fields

##### executionId (`!String`)
The unique id for the execution of creating a site

### createContentItem
Creates a content item

#### Input Fields

##### contentType (`!String`)
The content type, eg. `Blog` or `Page`

##### contentParts (`!String`)
A Json serialized list of content parts, eg.
```json
contentParts: "
  titlePart: { Title: "Dragonball Z" },
  bodyPart: { Text: "Rocks" }
"
```

#### Return Fields

##### contentItemId (`!String`)
The ids for the content is returned. You can use this id to query for the content item.



## Objects
Objects in GraphQL represent the resources you can access. An object can contain a list of fields, which are specifically typed.

For example, the Blog object has a field called contentItemId, which is a String.

Objects from the Orchard GraphQL query system are dynamically generated from the content type system.




## Resources

https://github.com/facebook/graphql
https://www.graph.cool/docs/reference/functions/request-pipeline/transform-input-arguments-caich7oeph/
http://graphql.org/learn/schema/#interfaces
https://blog.graph.cool/designing-powerful-apis-with-graphql-query-parameters-8c44a04658a9

This one! https://scaphold.io/community/blog/querying-relational-data-with-graphql/