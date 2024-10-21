using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DynamicCache;
using OrchardCore.DynamicCache.EventHandlers;
using OrchardCore.DynamicCache.Services;
using OrchardCore.DynamicCache.TagHelpers;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Extensions;
using OrchardCore.Localization;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement;

public class DynamicCacheTests
{
    private readonly ShapeTable _defaultShapeTable;
    private readonly TestShapeBindingsDictionary _additionalBindings;
    private readonly IServiceProvider _serviceProvider;

    public DynamicCacheTests()
    {
        _defaultShapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );
        _additionalBindings = [];

        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<IThemeManager, ThemeManager>();
        serviceCollection.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
        serviceCollection.AddScoped<ILoggerFactory, NullLoggerFactory>();
        serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
        serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();
        serviceCollection.AddScoped<IShapeBindingResolver, TestShapeBindingResolver>();
        serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
        serviceCollection.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
        serviceCollection.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        serviceCollection.AddLogging();

        serviceCollection.AddSingleton(_defaultShapeTable);
        serviceCollection.AddSingleton(_additionalBindings);
        serviceCollection.AddWebEncoders();

        serviceCollection.AddScoped<IDynamicCacheService, DefaultDynamicCacheService>();
        serviceCollection.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<IDynamicCacheService>());

        serviceCollection.AddScoped<IShapeDisplayEvents, DynamicCacheShapeDisplayEvents>();
        serviceCollection.AddShapeAttributes<CachedShapeWrapperShapes>();

        serviceCollection.AddSingleton<IDynamicCache, DefaultDynamicCache>();
        serviceCollection.AddSingleton<DynamicCacheTagHelperService>();
        serviceCollection.AddTagHelpers<DynamicCacheTagHelper>();
        serviceCollection.AddTagHelpers<CacheDependencyTagHelper>();

        serviceCollection.Configure<CacheOptions>(options => { });
        serviceCollection.Configure<DynamicCacheOptions>(options => { });

        serviceCollection.AddScoped<ITagCache, DefaultTagCache>();
        serviceCollection.AddScoped<ICacheContextManager, CacheContextManager>();
        serviceCollection.AddScoped<ICacheScopeManager, CacheScopeManager>();

        // IMemoryCache is registered at the tenant level so that there is one instance for each tenant.
        serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();

        // MemoryDistributedCache needs to be registered as a singleton as it owns a MemoryCache instance.
        serviceCollection.AddSingleton<IDistributedCache, MemoryDistributedCache>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private void AddShapeDescriptor(ShapeDescriptor shapeDescriptor)
    {
        _defaultShapeTable.Descriptors[shapeDescriptor.ShapeType] = shapeDescriptor;
        foreach (var binding in shapeDescriptor.Bindings)
        {
            _defaultShapeTable.Bindings[binding.Key] = binding.Value;
        }
    }

    private static DisplayContext CreateDisplayContext(IShape shape)
    {
        return new DisplayContext
        {
            Value = shape
        };
    }

    [Fact]
    public async Task ShapeResultsShouldBeInitialized()
    {
        var shapeType = "shapetype1";

        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var descriptor = new ShapeDescriptor
        {
            ShapeType = shapeType
        };
        descriptor.Bindings[shapeType] = new ShapeBinding
        {
            BindingName = shapeType,
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!"))
        };
        AddShapeDescriptor(descriptor);

        var factory = _serviceProvider.GetService<IShapeFactory>();
        var shapeResult = new ShapeResult(shapeType, ctx => factory.CreateAsync<MyModel>(shapeType, model => model.MyProperty = 2), shape =>
        {
            shape.Properties["property1"] = 1;
            ((MyModel)shape).MyProperty = 3;
            return Task.CompletedTask;
        }).Location("Content");

        var contentShape = await factory.CreateAsync("Content");

        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, "Detail", "", factory, null, null));

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));

        Assert.Equal("Hi there!", result.ToString());

        var modelShape = shapeResult.Shape as MyModel;
        Assert.NotNull(modelShape);
        Assert.Equal(3, modelShape.MyProperty);
        Assert.Equal(1, shapeResult.Shape.Properties["property1"]);
    }

    [Fact]
    public async Task ShapeResultsAreRenderedOnceWhenCached()
    {
        var shapeType = "shapetype1";
        var cacheTag = "mytag";

        var initializedCalled = 0;
        var processedCalled = 0;
        var bindCalled = 0;

        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var descriptor = new ShapeDescriptor
        {
            ShapeType = shapeType
        };
        descriptor.Bindings[shapeType] = new ShapeBinding
        {
            BindingName = shapeType,
            BindingAsync = ctx =>
            {
                bindCalled++;
                return Task.FromResult<IHtmlContent>(new HtmlString("Hi there!"));
            }
        };
        AddShapeDescriptor(descriptor);

        var factory = _serviceProvider.GetService<IShapeFactory>();

        ShapeResult CreateShapeResult() => new ShapeResult(
            shapeType,
            shapeBuilder: ctx => factory.CreateAsync<MyModel>(shapeType, model => model.MyProperty = 7),
            initializing: shape =>
            {
                initializedCalled++;
                return Task.CompletedTask;
            }).Location("Content").Cache("mycontent", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(1)).AddTag(cacheTag))
            .Processing(shape =>
            {
                processedCalled++;
                return Task.CompletedTask;
            });

        var shapeResult = CreateShapeResult();
        var contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, "Detail", "", factory, null, null));

        // Shape is created and initialized.
        Assert.Equal(7, ((MyModel)shapeResult.Shape).MyProperty);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));

        Assert.Equal("Hi there!", result.ToString());

        // Shape is rendered once.
        Assert.Equal(1, bindCalled);
        Assert.Equal(1, initializedCalled);

        for (var i = 1; i <= 10; i++)
        {
            // Create new ShapeResult.
            shapeResult = CreateShapeResult();
            contentShape = await factory.CreateAsync("Content");
            await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, "Detail", "", factory, null, null));

            result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));

            // Shape is not rendered twice.
            Assert.Equal(1, bindCalled);
            Assert.Equal(1, processedCalled);
            Assert.Equal(i + 1, initializedCalled);
            Assert.Equal("Hi there!", result.ToString());
        }

        // Invalidate cache
        var tagCache = _serviceProvider.GetService<ITagCache>();
        await tagCache.RemoveTagAsync(cacheTag);

        // Create new ShapeResult.
        shapeResult = CreateShapeResult();
        contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, "Detail", "", factory, null, null));

        result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));

        // Shape is processed and rendered again.
        Assert.Equal(2, bindCalled);
        Assert.Equal(2, processedCalled);
        Assert.Equal(12, initializedCalled);
        Assert.Equal("Hi there!", result.ToString());
    }

    [Fact]
    public async Task DriverResultsAssignProcessing()
    {
        // ShapeMetadata.Processing is the method which is not invoked when the shape is cached.
        // We need to ensure that the delegate used in the driver InitializeAsync call is used as Processing and not during
        // the creation of the shape.

        var factory = _serviceProvider.GetService<IShapeFactory>();

        async Task<IDisplayResult> CreateDisplayResultAsync()
        {
            var contentItem = new ContentItem();
            var contentShape = await factory.CreateAsync("Content");
            var buildDisplayContext = new BuildDisplayContext(contentShape, "Detail", "", factory, null, null);
            var driverResult = new MyDisplayDriver().Display(contentItem, buildDisplayContext);
            await driverResult.ApplyAsync(buildDisplayContext);

            return driverResult;
        }

        var displayResult = await CreateDisplayResultAsync();

        var shapeResult = displayResult as ShapeResult;

        Assert.NotNull(shapeResult);
        Assert.NotNull(shapeResult.Shape.Metadata.ProcessingAsync);
    }

    public class MyModel
    {
        public int MyProperty { get; set; } = 3;
    }

    public sealed class MyDisplayDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model, BuildDisplayContext context)
        {
            return Initialize<MyModel>(model => model.MyProperty++).Location("Content").Cache("id", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(1)));
        }
    }
}
