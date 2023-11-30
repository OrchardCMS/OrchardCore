namespace OrchardCore.Tests.DependencyInjection;

public class KeyedServices
{
    [Fact]
    public void AbleToResolveKeyedServiceAsDictionary()
    {
        var services = new ServiceCollection();

        services.AddOrchardCore();

        services.AddKeyedTransient<IService, Service1>("a");
        services.AddKeyedTransient<IService, Service2>("b");
        services.AddKeyedTransient<IService, Service3>("c");

        var serviceProvider = services.BuildServiceProvider();

        var keyService = serviceProvider.GetService<IDictionary<string, IService>>();

        Assert.NotNull(keyService);
        Assert.Equal(3, keyService.Count);
        Assert.True(keyService.ContainsKey("a"));
        Assert.True(keyService.ContainsKey("b"));
        Assert.True(keyService.ContainsKey("c"));
    }

    [Fact]
    public void AbleToResolveKeyedServiceAsReadOnlyDictionary()
    {
        var services = new ServiceCollection();

        services.AddOrchardCore();

        services.AddKeyedTransient<IService, Service1>("a");
        services.AddKeyedTransient<IService, Service2>("b");
        services.AddKeyedTransient<IService, Service3>("c");

        var serviceProvider = services.BuildServiceProvider();

        var keyService = serviceProvider.GetService<IReadOnlyDictionary<string, IService>>();

        Assert.NotNull(keyService);
        Assert.Equal(3, keyService.Count);
        Assert.True(keyService.ContainsKey("a"));
        Assert.True(keyService.ContainsKey("b"));
        Assert.True(keyService.ContainsKey("c"));
    }
}

public class IService { }

public class Service1 : IService { }

public class Service2 : IService { }

public class Service3 : IService { }
