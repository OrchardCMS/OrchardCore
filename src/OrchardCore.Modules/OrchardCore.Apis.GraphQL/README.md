# GraphQL API

The JsonApi implrements the jsonapi.org specification.

## Query
Every GraphQL schema has a root type for both queries and mutations. The query type defines GraphQL operations that retrieve data from the server.

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