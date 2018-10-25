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
  blog(contentItemId: "4k5df0kadp9asy1n2ejzs1rz4r") {
    displayText
  }
}
```

Here we can see that the query is using the arugment `contentItemId` to filter.

### Define a type

You can define a type in two ways, through either DI registration or Auto Discovery

#### DI Registration.

All query types registered in DI must inherit off the class `QueryFieldType`. 

Once you do this, you can then use the extension AddGraphQueryType to regiser it.

```c#
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services)
    {
        services.AddGraphQueryType<FooQueryType>();
    }
}
```

Once this is done, it will then display within your schema, and will be querable.

#### Auto Discovery

Auto discovery allows you to define Query Types outside of DI. This is specifically useful if you have dynamic content. With in Orchard for example, you can create a new content type without a static representation for that type.

To use this method, you must implement the interface `IQueryFieldTypeProvider`. The Field Types returned will be added to the Schema at runtime, and are not registered in DI. You will however need to register your class that implements `IQueryFieldTypeProvider` in DI.

### Define a Query Argument

Query Arguments and Field Return values are very similar, however there are cases where the seperation comes in handy.

To add a query argument, you inherit off the class `QueryArgumentObjectGraphType`.

Here is an example where im exposing the ability to query by the `alias` field on the AliasPart

```c#
public class AliasInputObjectType : QueryArgumentObjectGraphType<AliasPart>
{
    public AliasInputObjectType()
    {
        Name = "AliasPartInput";

        AddInputField("alias", x => x.Alias, true);
    }
}
```

### Define a return value

Return values are the values that are returned as part of a query.

To add return values, you inherit off the class `ObjectGraphType`

Here is an example where im exposing the ability to return the values `ListContentItemId` and `Order`.

```c#
public class ContainedQueryObjectType : ObjectGraphType<ContainedPart>
{
    public ContainedQueryObjectType()
    {
        Name = "ContainedPart";

        Field(x => x.ListContentItemId);
        Field(x => x.Order);
    }
}
```

## Mutations

Mutations are a way of manipulating data, rather that just querying.

To create a mutation you start by inheriting off `MutationFieldType`.
