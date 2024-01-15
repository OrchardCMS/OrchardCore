namespace OrchardCore.DependencyInjection.Tests;

public class ServiceProviderExtensionsTests
{
    [Fact]
    public void ResolveKeyedServiceDictionary()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new FooService());
        services.AddKeyedSingleton("foo", new FooService());
        services.AddKeyedSingleton("bar", new FooService());
        services.AddKeyedSingleton("baz", new FooService());

        var serviceProviderFactory = new OrchardCoreServiceProviderFactory();
        var serviceProvider = serviceProviderFactory.CreateServiceProvider(services);

        // Act
        var keyedDictionary = serviceProvider.GetKeyedServiceDictionary<FooService>();

        // Assert
        Assert.Equal(3, keyedDictionary.Count);
        Assert.Contains(keyedDictionary, item => item.Key.Equals("foo"));
        Assert.Contains(keyedDictionary, item => item.Key.Equals("bar"));
        Assert.Contains(keyedDictionary, item => item.Key.Equals("baz"));
        Assert.IsType<FooService>(keyedDictionary["foo"]);
        Assert.IsType<FooService>(keyedDictionary["bar"]);
        Assert.IsType<FooService>(keyedDictionary["baz"]);
    }
}
