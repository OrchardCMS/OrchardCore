# Testing with Orchard Core

When developing an OrchardCore solutions, most of the times you want to unit test the most mandatory parts of your application/module.
That's why we provide a testing infrastructure APIs, that will allow you to get started easily.

The best way to see how it works, is by looking at `OrchardCore.Tests` project in the source code.

## Unit Tests

For unit testing, `OrchardCore.Testing` provides a lot of mocks ans stubs that accelerate the process of testing your application/module. Moreover it contains helpers and utility classes that might help during writing unit tests.

### UseCultureAttribute

With the help of `UseCultureAttribute` you scope your method under test (MUT) to run on a specific culture, which is quite useful in many cases.

```csharp
[Fact]
[UseCulture("ar-YE")]
public void UnitTestName()
{
    // Omitted for brivety
}
```

## Integration Tests

For integration testing, `OrchardCore.Testing` provides some classes that accelerate the process of testing your application/module.

### SiteContextBase&lt;TSiteStartup&gt;

You create a `SiteContext` object and configure it by using the `SiteContextOptions` for building a site that using a certain database and recipe.
