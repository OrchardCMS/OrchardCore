# GraphQL

## Queries

Queries are made up of three areas, the `type`, `arguments` and `return values`, an example would be;

```json
{
  blog {
    displayText
  }
}
```

In this example, the `blog` is the type, and the `displayText` is the return value. You could expand this, to add an argument. An argument is used for filtering a query, for example;

```json
{
  blog(where: {contentItemId: "4k5df0kadp9asy1n2ejzs1rz4r"}) {
    displayText
  }
}
```

Here we can see that the query is using the argument `contentItemId` to filter.

### Define a query type

Lets see how to wire a type in to the GraphQL schema;

First: Lets start with a simple C# part

```csharp
public class AutoroutePart : ContentPart
{
    public string Path { get; set; }
    public bool SetHomepage { get; set; }
}
```

This is the part that is attached to your content item. GraphQL doesn't know what this is, so we now need to create a GraphQL representation of this class;

```csharp
public class AutorouteQueryObjectType : ObjectGraphType<AutoroutePart>
{
    public AutorouteQueryObjectType()
    {
        Name = "AutoroutePart";

        // Map the fields you want to expose
        Field(x => x.Path);
    }
}
```

There are two things going on here;

1. Inherit off `ObjectGraphType`. GraphQL understands this type.
2. Field(x => x.Path);. This tells the class from #1 what fields you want exposed publicly.

The last part is to tell the Orchard Subsystem about your new type, once this is done, the GraphQL subsystem will pick up your new object from its dependency tree. To do this, simple register it in a Startup class;

```csharp
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public class sealed Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // I have omitted the registering of the AutoroutePart, as we expect that to already be registered
        services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
    }
}
```

That's it, your part will now be exposed in GraphQL... just go to the query explorer and take a look. Magic.

## Filtration

### Define a custom query filter type

So now you have lots of data coming back, the next thing you want to do is to be able to filter said data.

We follow a similar process from step #1, so at this point I will make the assumption you have implemented step #1.

Use this approach if you:

- want to add a new filter on Content-Type queries,
- need to use custom logic for filtering. For example, fetching data from a service, or comparing complex objects.

What we are going to cover here is:

1. Implement an Input type.
2. Register it in Startup class.
3. Implement a Filter.

So, lets start. The Input type is similar to the Query type, here we want to tell the GraphQL schema that we accept this input.

```csharp
public class AutorouteInputObjectType : InputObjectGraphType<AutoroutePart>
{
    public AutorouteInputObjectType()
    {
        Name = "AutoroutePartInput";

        Field(x => x.Path, nullable: true).Description("the path of the content item to filter");
    }
}
```

Update Startup class like below.

```csharp
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // I have omitted the registering of the AutoroutePart, as we expect that to already be registered
        services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
        services.AddInputObjectGraphType<AutoroutePart, AutorouteInputObjectType>();
    }
}
```

The main thing to take away from this class is that all Input Types must inherit off of InputObjectGraphType.

When an input part is registered, it adds in that part as the parent query, in this instance the autoroutePart, as shown below;

```json
{
  blog(autoroutePart: { path: "somewhere" }) {
    displayText
  }
}
```

Next, we want to implement a filter. The filter takes the input from the class we just built and the above example, and performs the actual filter against the object passed to it. Note that `GraphQLFilter` also provides `PostQueryAsync` that can be used in other use cases too, like checking permissions.

```csharp
public class AutoroutePartGraphQLFilter : GraphQLFilter<ContentItem>
{
    public override Task<IQuery<ContentItem>> PreQueryAsync(IQuery<ContentItem> query, ResolveFieldContext context)
    {
        if (!context.HasArgument("autoroutePart"))
        {
            return Task.FromResult(query);
        }

        var part = context.GetArgument<AutoroutePart>("autoroutePart");

        if (part == null)
        {
            return Task.FromResult(query);
        }

        var autorouteQuery = query.With<AutoroutePartIndex>();

        if (!string.IsNullOrWhiteSpace(part.Path))
        {
            // Do not use commands that are terminating query, e.g. All() in here. Query needs to be editable, because ContentItemsFieldType that calls PreQueryAsync might need to work with it (e.g. insert another where conditions).

            return Task.FromResult(autorouteQuery.Where(index => index.Path == part.Path));
        }

        return Task.FromResult(query);
    }
}
```

The first thing we notice is

> context.GetArgument<AutoroutePart>("autoroutePart");

Shown in the example above, we have an autoroutePart argument, this is registered when we register an input type. From there we can deserialize and perform the query;

```json
{
  blog(autoroutePart: { path: "somewhere" }) {
    displayText
  }
}
```

Done.

### Using default Content-Type query filters

In the previous section, we demonstrated how to create filters for complex requirements, allowing you to create custom filtration methods. However, in case you need to add a simple filter on Content-Type queries, there is also a simpler solution.

Use this approach if you:

* want to add a new filter on Content-Type queries,
* will have a database index with data for your filters,
* you can use simple comparison (equals, contains, in...) against index values. For example, `AutoroutePartIndex.Path = filterValue`.

We will cover:

1. Implementing a `WhereInputObjectGraphType`.
2. Implementing `IIndexAliasProvider`.
3. Registering it in the `Startup` class.

#### Implementing WhereInputObjectGraphType

The `WhereInputObjectGraphType` enhances the `InputObjectGraphType` by introducing methods to define filters such as equality, substrings, or array filters. Inheriting from `WhereInputObjectGraphType` is essential since it's the expected type for the `ContentItemsFieldType` that is responsible for the filtering logic.

Here is an example implementation:

```csharp
// Assuming we've added the necessary using directives. 
// It is essential to inherit from WhereInputObjectGraphType. 
// Do not use the InputObjectGraphType type as it will not be 
// handled by default ContentItem queries.
public class AutorouteInputObjectType : WhereInputObjectGraphType<AutoroutePart>
{
    // Binds the filter fields to the GraphQL type representing AutoroutePart
    public AutorouteInputObjectType()
    {
        Name = "AutoroutePartInput";

        // Utilize the method for adding scalar fields from the base class.
        AddScalarFilterFields<StringGraphType>("path", S["Filter by the path of the content item"]);
    }
}
```

This method will add scalar filters to all ContentItem queries, including your own custom Content Types. Scalar filters include following:

1. equals, not equals
2. contains, not contains
3. starts with, ends with, not starts with, not ends with
4. in, not in

These filters are checked against an index that is bound to the given ```ContentPart```.

#### Implementing IIndexAliasProvider

To bind ```ContentPart``` to an Index, you have to implement ```IIndexAliasProvider```. Ensure that field names in your filter object are the same as fields in the index. It is needed for filter automatching.

```csharp
public class AutoroutePartIndexAliasProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases =
    [
        new IndexAlias
        {
            Alias = "autoroutePart", // alias of graphql ContentPart. You may also use nameof(AutoroutPart).ToFieldName()
            Index = nameof(AutoroutePartIndex), // name of index bound to part - keep in mind, that fields need to correspond. E.g. 'path' has the same name in the index and part.
            IndexType = typeof(AutoroutePartIndex)
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(_aliases);
    }
}
```

#### Updating the Startup Class

Update Startup class like below.

```csharp
[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    // Assuming we've added the necessary using directives.
    public override void ConfigureServices(IServiceCollection services)
    {
        // Code to register the AutoroutePart and AutorouteQueryObjectType is assumed to be present.
        // Register WhereInputObjectGraphType
        services.AddInputObjectGraphType<AutoroutePart, AutorouteInputObjectType>();

        // Register IIndexAliasProvider
        services.AddTransient<IIndexAliasProvider, AutoroutePartIndexAliasProvider>();
        services.AddWhereInputIndexPropertyProvider<AutoroutePartIndex>();
    }
}
```
With these configurations, you can navigate to your GraphQL interface, and you should see the new filters available for use in all Content type queries.

#### Example Query Filters

Below are the resulting query filters applied to an autoroutePart:

```json
{
  person(where: {path: {path_contains: "", path: "", path_ends_with: "", path_in: "", path_not: "", path_not_contains: "", path_not_ends_with: "", path_not_in: "", path_not_starts_with: "", path_starts_with: ""}}) {
    name
  }
}
```

Alternatively, if you register the part with ```collapse = true```, fields will not be nested inside object:

```json
{
  person(where: {path_contains: "", path: "", path_ends_with: "", path_in: "", path_not: "", path_not_contains: "", path_not_ends_with: "", path_not_in: "", path_not_starts_with: "", path_starts_with: ""}) {
    name
  }
}
```

For a more detailed understanding, refer to the implementation of [WhereInputObjectGraphType](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.Apis.GraphQL.Abstractions/Queries/WhereInputObjectGraphType.cs) and [ContentItemFieldsType](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/ContentItemsFieldType.cs). Also, you can check the [`AutoroutePartIndex`](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.Autoroute.Core/Indexes/AutoroutePartIndex.cs) that was used for examples.

### Using arguments for query filtering

There is also the possibility to utilize query arguments and use them for filtering query results, or customizing query output inside the `Resolve` method. For more information, visit the [GraphQL documentation](https://graphql-dotnet.github.io/docs/getting-started/arguments/).

Use this approach if you:

* want to add a new filter on any type of query, content part, or field,
* or will use custom logic for filtration.
* or you need to switch data sources, or logic, based on the argument's value

Orchard Core's implementation of a filtering query by an argument can be seen in [`ContentItemQuery`](https://github.com/OrchardCMS/OrchardCore/blob/main/src/OrchardCore/OrchardCore.ContentManagement.GraphQL/Queries/ContentItemQuery.cs) or `MediaAssetQuery`.

Orchard Core's implementation of applying an argument on a field can be seen in `MediaFieldQueryObjectType`.

## Querying related content items

One of the features of Content Items, is that they can be related to other Content Items.

Imagine we have the following Content Types: Movie (with name and ReleaseYear as text fields) and Person with a FavoriteMovies field (content picker field of Movie).

### Get the related content items GraphQL query

Now, if we would want to get the Favorite Movies of the Person items we query, the following query will throw an error:

```json
{
  person {
    name
    favoriteMovies {
      contentItems {
        name
        releaseYear
      }
    }
  }
}
```

The error will complain that ```name``` and ```releaseYear``` are not fields of a Content Item.

The ´inline fragment´ the error hints about, is a construct to tell the query parser what it is supposed to do with these generic items, and have them treated as a ´Movie´ type instead of as a generic ´Content Item´.

**Notice** the ```... on Movie``` fragment, that tells the GraphQL parser to treat the discovered object as ´Movie´.

The following query gives us the results we want:

```json
{
  person {
    name
    favoriteMovies {
      contentItems {
        ... on Movie {
          name
          releaseYear
        }
      }
    }
  }
}
```

## More Info

For more information on GraphQL you can visit the following links:

- <https://graphql.org/learn/>
- <https://graphql-dotnet.github.io/docs/getting-started/introduction>
