# Contents (`OrchardCore.Contents`)

This module provides Content Management services.

## CommonPart

Attach this part to your content items to edit the common properties like `CreatedUtc` and `Owner` of a `ContentItem`.

The following properties are available on `CommonPart`:

| Name | Type | Description |
| -----| ---- |------------ |
| `CreatedUtc` | `DateTime` | The HTML content in the body. |
| `Owner` | `string` | The HTML content in the body. |
| `Content` | | The raw content of the part. |
| `ContentItem` | | The content item containing this part. |

## Liquid

You can access content items from liquid views and templates by using the `Content` property.  
By default, you can retrieve content by alias or content item ID.  
Other modules (such as `Alias` and `Autoroute`) allow you to retrieve content by other identifiers.

### Loading from a handle

```liquid
{% assign my_content = Content["alias:main-menu"] %}
```

Handles can be in various forms, like when using Autoroute, with the `slug` prefix.

```liquid
{% assign my_content = Content["slug:my-blog/my-blog-post"] %}
```

> Handles are provided by implementing `IContentHandleProvider`.

### Loading the latest version of a content item

You can use the `Latest` property to retrieve the latest version of a content item (whether that's the published version or the latest draft version) by alias:

```liquid
{% assign my_content = Content.Latest["alias:main-menu"] %}
```

### Loading from a content item id

```liquid
{% assign my_content = Content.ContentItemId["417qsjrgv97e74wvp149h4da53"] %}
```

When a list of content item ids is available, the `content_item_id` filter should be preferred:

```liquid
{% assign posts = postIds | content_item_id %}
```

### Loading from a content item version id

```liquid
{% assign my_content = Content.ContentItemVersionId["49gq8g6zndfc736x0az3zsp4w3"] %}
```

### Rendering a content item from a handle

```liquid
{% contentitem handle:"alias:test", display_type="Summary" %}
```

The default display type is "Detail" when none is specified.
An optional `alternate` argument can be specified.

### Logging to the browser console

The `console_log` liquid filter can be used to dump data from well known properties, or objects serializable to json, to the browser console.

```liquid
{{ Model.Content | console_log }}
```

```liquid
{{ Model.ContentItem | console_log }}
```

Well known properties include

- Strings
- JTokens
- Content Items (from the `Model.ContentItem` property)
- Shapes (from the `Model.Content` property)
- Objects that can serialize to json.

!!! note
    To log shapes call `{{ Model.Content | console_log }}` after calling `{{ Model.Content | shape_render }}`
    This will allow the shape to execute, and populate the alternates for any child shapes.

## Razor Helper

The following methods are available from the Razor helper.

| Method | Parameters | Description |
| --------- | ---- |------------ |
| `GetContentItemIdByHandleAsync` | `string name` | Returns the content item id from its handle. Ex: `alias:carousel`, `slug:myblog/my-blog-post` |
| `GetContentItemByHandleAsync` | `string handle, bool latest = false` | Loads a content item from its handle, seeking the latest version or not. |
| `GetContentItemByIdAsync` | `string contentItemId, bool latest = false` | Loads a content item from its id, seeking the latest version or not. |
| `GetContentItemsByIdAsync` | `IEnumerable<string> contentItemIds, bool latest = false` | Loads a list of content items by ids, seeking the latest version or not. |
| `GetContentItemByVersionIdAsync` | `string contentItemVersionId` | Loads a content item from its version id. |
| `ConsoleLog` | `object content` | Logs content to the browser console |

> The Razor Helper is accessible on the `Orchard` property if the view is using Orchard Core's Razor base class, or by injecting `OrchardCore.IOrchardHelper` in all other cases.

### Razor console log

The `ConsoleLog` extension method can be used to dump data from well known properties, or objects serializable to json, to the browser console.

`@Orchard.ConsoleLog(Model.Content as object)` noting that we cast to an object, as extension methods do not support dynamic dispatching.

`@Orchard.ConsoleLog(Model.ContentItem as object)` noting that we cast to an object, as extension methods do not support dynamic dispatching.

Well known properties include

- Strings
- JTokens
- Content Items (from the `Model.ContentItem` property)
- Shapes (from the `Model.Content` property)
- Objects that can serialize to json.

!!! note
    To log shapes call `@Orchard.ConsoleLog(Model.Content as object)` after calling `@await DisplayAsync(Model.Content)`
    This will allow the shape to execute, and populate the alternates for any child shapes.

## Contents Module RESTful Web API

The `OrchardCore.Contents` module provides RESTful API endpoints via [`minimal API`](https://github.com/OrchardCMS/OrchardCore/tree/main/src/OrchardCore.Modules/OrchardCore.Contents/Endpoints/Api) featuring endpoints to manage _content items_. These endpoints allow for operations such as retrieving, creating, updating, and deleting single content item instances. Access to these endpoints requires authentication and appropriate user role permissions.

### Useful modules and libraries

- We would suggest you read the docs about the [GraphQL module](../Apis.GraphQL/README.md), to be used for querying content items.
- There's a [Swagger module](https://github.com/OrchardCoreContrib/OrchardCoreContrib.Modules/blob/main/src/OrchardCoreContrib.Apis.Swagger/README.md) made by the community, that allows you to create APIs documentation using Swagger.
- Lombiq provide a [client library](https://github.com/Lombiq/Orchard-Core-API-Client) for communicating with the Orchard Core web APIs.

### Activating the "OpenId Authorization Server" and "OpenId Token Validation" Features, and setting User Roles

To utilize the Orchard Core Contents API endpoints, user accounts must authenticate using the OAuth 2 standard by activating and configuring the "OpenId Authorization Server" and "OpenId Token Validation" features. Detailed configuration steps can be found in the [OpenId Authorization Server documentation](https://docs.orchardcore.net/en/main/docs/reference/modules/OpenId/#authorization-server) and the [OpenId Token Validation documentation](https://docs.orchardcore.net/en/main/docs/reference/modules/OpenId/#token-validation).

It is usually better to **create a dedicated user for performing API calls**, maintain control over user rights, and easily activate/deactivate the API user. The `OrchardCore.OpenId` feature allows setting these user role permissions from "Roles → Edit (User)". Those are the available permissions:

- View and edit the OpenID Connect client settings
- View and edit the OpenID Connect server settings
- View and edit the OpenID Connect validation settings
- View, add, edit, and remove the OpenID Connect applications
- View, add, edit, and remove the OpenID Connect scopes

### Contents API Controller Endpoints

#### GET /api/content/{contentItemId}

##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| contentItemId | path | The ID of the Content Item to be retrieved. | Yes | string |

##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |

***

#### POST /api/content

##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
|  | payload | The content item model to be updated | Yes | Json |

##### Body payload example

```json
{
  "contentItem": "string",
  "id": 0,
  "contentItemId": "string",
  "contentItemVersionId": "string",
  "contentType": "string",
  "published": true,
  "latest": true,
  "modifiedUtc": "2024-03-14T11:40:20.331Z",
  "publishedUtc": "2024-03-14T11:40:20.331Z",
  "createdUtc": "2024-03-14T11:40:20.331Z",
  "owner": "string",
  "author": "string",
  "displayText": "string"
}
```

> This payload example model was obtained using the GraphiQL panel available in the Admin: _Configuration_ → _GraphiQL_. In this [video](https://www.youtube.com/watch?v=8SbW3TLNhF0) you can find an overview of how to use GraphiQL. 

##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |

***

#### DELETE /api/content/{contentItemId}

##### Parameters

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| contentItemId | path | The ID of the Content Item to be deleted. | Yes | string |

##### Responses

| Code | Description |
| ---- | ----------- |
| 200 | Success |

## GraphQL

The contents module provides a feature to provide GraphQL queries for content items.
For more information about how to send GraphQL queries, please refer to [this section](../Apis.GraphQL/README.md).

### Content Type Queries

You can use content queries to fetch either a single content item, or a list of content items for a certain content type.

Here, we use the `blogPost` query to fetch a list of `BlogPost` content items. In the response, we include only the `contentItemId` and `displayText` of each `BlogPost` content item:

```graphql
query {
  blogPost {
    contentItemId
    displayText
  }
}
```

We can also query a specific `BlogPost` content item using the `blogPost` query. Note that we're using the `where` argument to select the content item:

```graphql
query {
  blosPost(where: {
    contentItemId: "417qsjrgv97e74wvp149h4da53"
  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

### Available fields

These fields are available at the content item level:

| Property |
| -------- |
| `contentItemId` |
| `contentItemVersionId` |
| `contentType` |
| `displayText` |
| `published` |
| `latest` |
| `modifiedUtc` |
| `publishedUtc` |
| `createdUtc` |
| `owner` |
| `author` |

In addition, all the content parts can also be retrieved like this:

```graphql
{
  blogPost {
    displayText
    autoroutePart {
      path
    }
  }
}
```

### Query arguments

Different types of query arguments can be composed to filter the results:

- Ordering: Sorting content items by any field value using `orderBy`
- Filtering: Selecting content items in a query by scalar or relational filters using `where`
- Pagination: Slicing content items in a query using `first` and `skip`

#### Ordering

When querying all content items of a type you can supply the `orderBy` argument for every scalar field of the type: `orderBy: { <field>: ASC }` or  `orderBy: { <field>: DESC }`.

Order the list of all `BlogPost` content items ascending by `displayText`:

```graphql
query {
  blogPost(orderBy: { displayText: ASC }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

Order the list of all `BlogPost` content items descending by `publishedUtc` and then ascending by `displayText`:

```graphql
query {
  blogPost(orderBy: { publishedUtc: DESC, displayText: ASC  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

The field you are ordering by does not have to be selected in the actual query.

It's also not currently possible to order responses by their parts or custom fields.

#### Filtering

When querying all content items of a type you can supply different parameters to the where argument to constrain the data in the response according to your requirements.
The available options depend on the scalar and part fields defined on the type in question.

##### Single Filters

If you supply exactly one parameter to the `where` argument, the query response will only contain content items that adhere to this constraint.  
Multiple filters can be combined using `AND` and/or `OR`, see [below](#arbitrary-combination-of-filters-with-and-or-and-not) for more details.

##### Filtering by a publication status

By default only the published content items are returned. You can select either `DRAFT`, `LATEST` or `ALL` versions of a content item.

```graphql
query {
  blogPost(status: DRAFT) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

##### Filtering by a concrete value

The easiest way to filter a query response is by supplying a concrete value for a certain field to filter by.

Query all `BlogPost` content items with a specific display text:

```graphql
query {
  blogPost(where: {
    displayText: "About"
  }) {
    contentItemId
  }
}
```

##### Advanced filter criteria

Depending on the type of the field you want to filter by, you have access to different advanced criteria you can use to filter your query response.

Query all `BlogPost` content items whose `displayText` is in a given list of strings:

```graphql
query {
  blogPost(where: {
    displayText_in: ["My biggest Adventure", "My latest Hobbies"]
  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

Query all `BlogPost` content items whose creation date is less than a specific date:

```graphql
query {
  blogPost(where: {
    creationgUtc_lt: "2011-11-13T07:45:00"
  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

##### Content Part Filters

For content parts, you can define conditions on the part by nesting the according argument in `where`.

Query all `BlogPost` content items where the `autoroutePart` has a specific value in its `path`:

```graphql
query {
  blogPost(where: {
    autoroutePart {
      path_contains: "/about"
    }
  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

##### Combining Multiple Filters

You can use the filter combinators `OR`, `AND` and `NOT` to create an arbitrary logical combination of filter conditions:

For an `AND`-filter to evaluate to `true`, all of the nested conditions have to be `true`.
For an `OR`-filter to evaluate to `true`, at least one of the nested conditions has to be `true`.
For a `NOT`-filter to evaluate to `true`, all of the nested conditions have to be `false`.
Using `OR`, `AND` and `NOT`

Let's start with an easy example:

Query all `BlogPost` content items that are created in `2018` and whose `displayText` is in a given list of strings:

```graphql
query {
  blogPost(where: {
    AND: {
      displayText_in: ["My biggest Adventure", "My latest Hobbies"]
    }, {
      publishedUtc_gt: "2018"
    }
  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

##### Arbitrary combination of filters with `AND`, `OR` and `NOT`

You can combine and even nest the filter combinators `AND`, `OR` and `NOT` to create arbitrary logical combinations of filter conditions.

Query all `BlogPost` content items that are created in `2018` and whose `displayText` is in a given list of strings, or have the specific `contentItemId` we supply:

```graphql
query {
  blogPost(where: {
    OR: {
      AND: {
        displayText_in: ["My biggest Adventure", "My latest Hobbies"],
        publishedUtc_gt: "2018"
      },

      contentItemId: "417qsjrgv97e74wvp149h4da53"
    }
  }) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

Notice how we nest the `AND` combinator inside the `OR` combinator.

#### Pagination

When querying all content items of a specific content type, you can supply arguments that allow you to paginate the query response.

##### Limiting the number of results

To limit the number of results, use `first`.

##### Skipping elements with skip

To skip a number of results, use `skip`.

##### Examples

Query the first 3 content items:

```graphql
query {
  posts(first: 3) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

Query the content items from position 6 to position 10:

```graphql
query {
  posts(
    first: 5
    skip: 5
  ) {
    contentItemId
    displayText
    publishedUtc
  }
}
```

## List Content Types by Stereotype

Starting with version 1.7, the Contents admin UI provides a way to manage content items of content types that share the same Stereotype.

For example, lets say we want list all content items of a content types that use `Test` stereotype. To do that, add an admin menu item that directs the user to `/Admin/Contents/ContentItems?stereotype=Test`. Adding `stereotype=Test` to the URL will render the UI using any content type that has `Test` as it's stereotype.

## Full-Text Search for Admin UI

Starting with version 1.7, a new options have been introduced to enable control over the behavior of the full-text search in the administration user interface for content items.

For instance, consider a content type called `Product.` Currently, when a user performs a search, the default behavior is to check if the search terms are present in the `DisplayText` column of the `ContentItemIndex` for the content item. However, what if a user wants to search for a product using its serial number, which is not part of the `DisplayText` field?

With the newly added options, we can now allow searching for products based on either the display text or the serial number. To modify the default behavior, two steps need to be taken:

 1. Implement `IContentsAdminListFilterProvider` interface by defining a custom lookup logic. For example:

    ```csharp
    public class ProductContentsAdminListFilterProvider : IContentsAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<ContentItem> builder)
        {
            builder
                .WithNamedTerm("producttext", builder => builder
                    .ManyCondition(
                        (val, query) => query.Any(
                            (q) => q.With<ContentItemIndex>(i => i.DisplayText != null && i.DisplayText.Contains(val)),
                            (q) => q.With<ProductIndex>(i => i.SerialNumber != null && i.SerialNumber.Contains(val))
                        ),
                        (val, query) => query.All(
                            (q) => q.With<ContentItemIndex>(i => i.DisplayText == null || i.DisplayText.NotContains(val)),
                            (q) => q.With<ProductIndex>(i => i.SerialNumber == null || i.SerialNumber.NotContains(val))
                        )
                    )
                );
        }
    }
    ```

 2. Register the custom default term name as a search option by adding it to the `ContentsAdminListFilterOptions.` For example:

    ```csharp
    services.Configure<ContentsAdminListFilterOptions>(options =>
    {
        options.DefaultTermNames.Add("Product", "producttext");
    });
    ```

Now, when a user searches for a product's serial number in the administration UI, we will utilize the `producttext` filter instead of the default `text` filter to perform the search.

The `UseExactMatch` option in the `ContentsAdminListFilterOptions` class modifies the default search behavior by enclosing searched terms within quotation marks, creating an exact match search by default, this unless if the search text explicitly uses 'OR' or 'AND' operators.


## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/j6xuupq9FYY" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/wbTEUl_N0Lk" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
