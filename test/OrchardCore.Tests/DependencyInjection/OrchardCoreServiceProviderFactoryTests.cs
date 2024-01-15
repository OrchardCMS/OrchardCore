namespace OrchardCore.DependencyInjection.Tests;

public class OrchardCoreServiceProviderFactoryTests
{
    [Fact]
    public void ResolveKeys()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new FooService());
        services.AddKeyedSingleton("foo", new FooService());
        services.AddKeyedSingleton("bar", new FooService());
        services.AddKeyedSingleton("baz", new FooService());

        // Act
        var serviceProviderFactory = new OrchardCoreServiceProviderFactory();
        var serviceProvider = serviceProviderFactory.CreateServiceProvider(services);

        // Assert
        var keys = serviceProvider.GetRequiredService<Keys<FooService>>();
        Assert.Equal(3, keys.Count);
        Assert.Equal("foo", keys[0]);
        Assert.Equal("bar", keys[1]);
        Assert.Equal("baz", keys[2]);
    }
}
