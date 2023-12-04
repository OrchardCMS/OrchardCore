using OrchardCore.Extensions;

namespace OrchardCore.Tests.DependencyInjection;

public class KeyedServicesTests
{
    [Fact]
    public void AbleToResolveKeyedServiceAsIKeyedServiceDictionary()
    {
        var services = new ServiceCollection();

        services.AddOrchardCore();

        services.AddKeyedTransient<IService, Service1>("a")
             .AddKeyedTransient<IService, Service2>("b")
             .AddKeyedTransient<IService, Service3>("c")
             .AddKeyedTransient<IService, Service2>("a");

        var serviceProvider = services.BuildServiceProvider();

        var keyService = serviceProvider.GetService<IKeyedServiceDictionary<string, IService>>();

        Assert.NotNull(keyService);

        Assert.True(keyService.ContainsKey("a"));
        Assert.True(keyService.ContainsKey("b"));
        Assert.True(keyService.ContainsKey("c"));

        Assert.Equal(3, keyService.Count);

        // Assert we resolve the last service.
        Assert.True(keyService["a"].GetType() == typeof(Service2));
    }

    [Fact]
    public void AbleToResolveKeyedServiceFromKeyedResolver()
    {
        var services = new ServiceCollection();

        services.AddOrchardCore();

        services.AddKeyedTransient<IService, Service1>("a")
             .AddKeyedTransient<IService, Service2>("b")
             .AddKeyedTransient<IService, Service3>("c")
             .AddKeyedTransient<IService, Service2>("a");

        var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetService<IKeyedServiceResolver>();
        var keyService = resolver.GetServicesAsDictionary<string, IService>();

        Assert.NotNull(keyService);
        Assert.True(keyService.ContainsKey("a"));
        Assert.True(keyService.ContainsKey("b"));
        Assert.True(keyService.ContainsKey("c"));

        Assert.Equal(3, keyService.Count);

        // Assert we resolve the last service.
        Assert.True(keyService["a"].GetType() == typeof(Service2));
    }

    [Fact]
    public void AbleToResolveKeyedServiceAllKeyedServices()
    {
        var services = new ServiceCollection();

        services.AddOrchardCore();
        services.AddKeyedTransient<IService, Service1>("a")
            .AddKeyedTransient<IService, Service2>("b")
            .AddKeyedTransient<IService, Service3>("c")
            .AddKeyedTransient<IService, Service2>("a");

        var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetService<IKeyedServiceResolver>();
        var keyService = resolver.GetServices<string, IService>();

        Assert.NotNull(keyService);
        Assert.Contains(keyService, x => x.Key == "a");
        Assert.Contains(keyService, x => x.Key == "b");
        Assert.Contains(keyService, x => x.Key == "c");
        Assert.Equal(4, keyService.Count());
        Assert.Equal(2, keyService.Where(x => x.Key == "a").Count());
    }
}

public class IService { }

public class Service1 : IService { }

public class Service2 : IService { }

public class Service3 : IService { }
