# Json Apis (Orchard.JsonApi)

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