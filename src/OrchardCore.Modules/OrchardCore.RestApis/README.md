# Json Apis (OrchardCore.RestApis)

The JsonApi implrements the jsonapi.org specification.

## Query

The content types in orchard are not pluralized, an therefore they are not pluralized through the api.

### All content of content type
To get all content of a particular content type, in this example a Blog

/api/contents/blog

### Content by Id
To get all content of a particular Id you must know its type, so if the blog has an id of 10, it will be

/api/contents/blog/10

### Nested content
A content type might implement a content part that contains nested content, to query this you do this

/api/contents/blog/10/relationships/blogpost

# Orchard GraphQL structure

Orchard
  ContentItems
    ContentType
	ContentParts

## Queries and Mutations

### Fields

Query
```json
query {
    contentItem {
        contentItemId
    }
}
```

Produces
```json
query {
    contentItem {
        SFDSKF823RK3O
    }
}
```

## Resources

https://github.com/facebook/graphql
https://www.graph.cool/docs/reference/functions/request-pipeline/transform-input-arguments-caich7oeph/
http://graphql.org/learn/schema/#interfaces
https://blog.graph.cool/designing-powerful-apis-with-graphql-query-parameters-8c44a04658a9

This one! https://scaphold.io/community/blog/querying-relational-data-with-graphql/