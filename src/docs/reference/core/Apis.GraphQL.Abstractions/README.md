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

This is the part that is attached to your content item. GraphQL doesnt know what this is, so we now need to create a GraphQL representation of this class;

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
public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // I have omitted the registering of the AutoroutePart, as we expect that to already be registered
        services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
    }
}
```

Thats it, your part will now be exposed in GraphQL... just go to the query explorer and take a look. Magic.

### Define a query filter type

So now you have lots of data coming back, the next thing you want to do is to be able to filter said data.

We follow a similar process from step #1, so at this point I will make the assumption you have implemented step #1.

What we are going to cover here is;

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
public class Startup : StartupBase
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

Next we want to implement a filter. The filter takes the input from the class we just built and the above example, and performs the actual filter against the object passed to it.

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
            return Task.FromResult(autorouteQuery.Where(index => index.Path == part.Path).All());
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
