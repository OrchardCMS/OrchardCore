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
        serviceCollection.AddScoped<IDisplayHelper, DisplayHelper>();
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

    private DisplayContext CreateDisplayContext(IShape shape)
    {
        return new DisplayContext
        {
            Value = shape,
            ServiceProvider = _serviceProvider,
        };
    }

    [Fact]
    public async Task ShapeResultsShouldBeInitialized()
    {
        var shapeType = "shapetype1";

        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var descriptor = new ShapeDescriptor
        {
            ShapeType = shapeType,
        };
        descriptor.Bindings[shapeType] = new ShapeBinding
        {
            BindingName = shapeType,
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
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

        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

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
            ShapeType = shapeType,
        };
        descriptor.Bindings[shapeType] = new ShapeBinding
        {
            BindingName = shapeType,
            BindingAsync = ctx =>
            {
                bindCalled++;
                return Task.FromResult<IHtmlContent>(new HtmlString("Hi there!"));
            },
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
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

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
            await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

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
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

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
            var buildDisplayContext = new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null);
            var driverResult = new MyDisplayDriver().Display(contentItem, buildDisplayContext);
            await driverResult.ApplyAsync(buildDisplayContext);

            return driverResult;
        }

        var displayResult = await CreateDisplayResultAsync();

        var shapeResult = displayResult as ShapeResult;

        Assert.NotNull(shapeResult);
        Assert.NotNull(shapeResult.Shape.Metadata.ProcessingAsync);
    }

    [Fact]
    public async Task ShapeMorphingWithCachingCachesFinalMorphedShape()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var factory = _serviceProvider.GetService<IShapeFactory>();

        var originalBindingCalled = 0;
        var morphedBindingCalled = 0;
        var cacheTag = "morphing-test";

        // Original shape descriptor that morphs to another shape
        var originalDescriptor = new ShapeDescriptor
        {
            ShapeType = "OriginalShape",
        };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                originalBindingCalled++;
                
                // Morph the shape to a different type
                ((IShape)ctx.Value).Metadata.Type = "MorphedShape";
                
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        // Morphed shape descriptor
        var morphedDescriptor = new ShapeDescriptor
        {
            ShapeType = "MorphedShape",
        };
        morphedDescriptor.Bindings["MorphedShape"] = new ShapeBinding
        {
            BindingName = "MorphedShape",
            BindingAsync = ctx =>
            {
                morphedBindingCalled++;
                return Task.FromResult<IHtmlContent>(new HtmlString("Morphed Content"));
            },
        };
        AddShapeDescriptor(morphedDescriptor);

        // Create cached shape result that morphs
        var shapeResult = new ShapeResult("OriginalShape", ctx => factory.CreateAsync("OriginalShape"))
            .Location("Content")
            .Cache("morphed-shape", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        var contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        // First execution - should trigger morphing and cache the result
        var result1 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Morphed Content", result1.ToString());
        Assert.Equal(1, originalBindingCalled);
        Assert.Equal(1, morphedBindingCalled);
        Assert.Equal("MorphedShape", shapeResult.Shape.Metadata.Type);

        // Second execution - should use cached result, no morphing should occur
        shapeResult = new ShapeResult("OriginalShape", ctx => factory.CreateAsync("OriginalShape"))
            .Location("Content")
            .Cache("morphed-shape", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result2 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Morphed Content", result2.ToString());
        Assert.Equal(1, originalBindingCalled); // Should not increase
        Assert.Equal(1, morphedBindingCalled); // Should not increase

        // Invalidate cache and verify morphing happens again
        var tagCache = _serviceProvider.GetService<ITagCache>();
        await tagCache.RemoveTagAsync(cacheTag);

        shapeResult = new ShapeResult("OriginalShape", ctx => factory.CreateAsync("OriginalShape"))
            .Location("Content")
            .Cache("morphed-shape", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result3 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Morphed Content", result3.ToString());
        Assert.Equal(2, originalBindingCalled); // Should increase after cache invalidation
        Assert.Equal(2, morphedBindingCalled); // Should increase after cache invalidation
    }

    [Fact]
    public async Task ShapeMorphingChainWithCachingCachesFinalResult()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var factory = _serviceProvider.GetService<IShapeFactory>();

        var firstBindingCalled = 0;
        var secondBindingCalled = 0;
        var finalBindingCalled = 0;
        var cacheTag = "morphing-chain-test";

        // First shape that morphs to second
        var firstDescriptor = new ShapeDescriptor { ShapeType = "FirstShape" };
        firstDescriptor.Bindings["FirstShape"] = new ShapeBinding
        {
            BindingName = "FirstShape",
            BindingAsync = async ctx =>
            {
                firstBindingCalled++;
                var shape = (IShape)ctx.Value;
                shape.Metadata.Type = "SecondShape";
                shape.Properties["MorphCount"] = 1;
                
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync(shape);
            },
        };
        AddShapeDescriptor(firstDescriptor);

        // Second shape that morphs to final
        var secondDescriptor = new ShapeDescriptor { ShapeType = "SecondShape" };
        secondDescriptor.Bindings["SecondShape"] = new ShapeBinding
        {
            BindingName = "SecondShape",
            BindingAsync = async ctx =>
            {
                secondBindingCalled++;
                var shape = (IShape)ctx.Value;
                shape.Metadata.Type = "FinalShape";
                var morphCount = (int)shape.Properties["MorphCount"];
                shape.Properties["MorphCount"] = morphCount + 1;
                
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync(shape);
            },
        };
        AddShapeDescriptor(secondDescriptor);

        // Final shape that renders content
        var finalDescriptor = new ShapeDescriptor { ShapeType = "FinalShape" };
        finalDescriptor.Bindings["FinalShape"] = new ShapeBinding
        {
            BindingName = "FinalShape",
            BindingAsync = ctx =>
            {
                finalBindingCalled++;
                var shape = (IShape)ctx.Value;
                var morphCount = (int)shape.Properties["MorphCount"];
                return Task.FromResult<IHtmlContent>(new HtmlString($"Final Content (Morphed {morphCount} times)"));
            },
        };
        AddShapeDescriptor(finalDescriptor);

        // Create cached shape result that morphs through chain
        var shapeResult = new ShapeResult(
            "FirstShape",
            ctx => factory.CreateAsync("FirstShape"))
            .Location("Content")
            .Cache("morphed-chain", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        var contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        // First execution - should go through entire morphing chain
        var result1 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Final Content (Morphed 2 times)", result1.ToString());
        Assert.Equal(1, firstBindingCalled);
        Assert.Equal(1, secondBindingCalled);
        Assert.Equal(1, finalBindingCalled);
        Assert.Equal("FinalShape", shapeResult.Shape.Metadata.Type);

        // Second execution - should use cached result, no morphing should occur
        shapeResult = new ShapeResult(
            "FirstShape",
            ctx => factory.CreateAsync("FirstShape"))
            .Location("Content")
            .Cache("morphed-chain", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result2 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Final Content (Morphed 2 times)", result2.ToString());
        Assert.Equal(1, firstBindingCalled); // Should not increase
        Assert.Equal(1, secondBindingCalled); // Should not increase
        Assert.Equal(1, finalBindingCalled); // Should not increase

        // Invalidate cache and verify morphing happens again
        var tagCache = _serviceProvider.GetService<ITagCache>();
        await tagCache.RemoveTagAsync(cacheTag);

        shapeResult = new ShapeResult(
            "FirstShape",
            ctx => factory.CreateAsync("FirstShape"))
            .Location("Content")
            .Cache("morphed-chain", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result3 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Final Content (Morphed 2 times)", result3.ToString());
        Assert.Equal(2, firstBindingCalled); // Should increase after cache invalidation
        Assert.Equal(2, secondBindingCalled); // Should increase after cache invalidation
        Assert.Equal(2, finalBindingCalled); // Should increase after cache invalidation

        // Verify it caches again
        var result4 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Final Content (Morphed 2 times)", result4.ToString());
        Assert.Equal(2, firstBindingCalled); // Should not increase
        Assert.Equal(2, secondBindingCalled); // Should not increase
        Assert.Equal(2, finalBindingCalled); // Should not increase
    }

    [Fact]
    public async Task ConditionalShapeMorphingWithCachingRespectsCacheKeys()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var factory = _serviceProvider.GetService<IShapeFactory>();

        var conditionalBindingCalled = 0;
        var morphedBindingCalled = 0;
        var cacheTag = "conditional-morphing-test";

        // Conditional shape that morphs based on properties
        var conditionalDescriptor = new ShapeDescriptor { ShapeType = "ConditionalShape" };
        conditionalDescriptor.Bindings["ConditionalShape"] = new ShapeBinding
        {
            BindingName = "ConditionalShape",
            BindingAsync = async ctx =>
            {
                conditionalBindingCalled++;
                var shape = (IShape)ctx.Value;
                var shouldMorph = shape.Properties.TryGetValue("ShouldMorph", out var morphValue) && (bool)morphValue;
                
                if (shouldMorph)
                {
                    shape.Metadata.Type = "MorphedConditionalShape";
                    var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                    return await displayHelper.ShapeExecuteAsync(shape);
                }
                
                return new HtmlString("Original Conditional Content");
            },
        };
        AddShapeDescriptor(conditionalDescriptor);

        // Morphed shape
        var morphedDescriptor = new ShapeDescriptor { ShapeType = "MorphedConditionalShape" };
        morphedDescriptor.Bindings["MorphedConditionalShape"] = new ShapeBinding
        {
            BindingName = "MorphedConditionalShape",
            BindingAsync = ctx =>
            {
                morphedBindingCalled++;
                return Task.FromResult<IHtmlContent>(new HtmlString("Morphed Conditional Content"));
            },
        };
        AddShapeDescriptor(morphedDescriptor);

        // Test morphing case (ShouldMorph = true)
        var shapeResult1 = new ShapeResult(
            "ConditionalShape",
            ctx => factory.CreateAsync("ConditionalShape"), 
            shape => 
            { 
                shape.Properties["ShouldMorph"] = true; 
                return Task.CompletedTask; 
            })
            .Location("Content")
            .Cache("conditional-morph-true", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        var contentShape = await factory.CreateAsync("Content");
        await shapeResult1.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result1 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult1.Shape));
        
        Assert.Equal("Morphed Conditional Content", result1.ToString());
        Assert.Equal(1, conditionalBindingCalled);
        Assert.Equal(1, morphedBindingCalled);

        // Test non-morphing case (ShouldMorph = false) - different cache key
        var shapeResult2 = new ShapeResult(
            "ConditionalShape",
            ctx => factory.CreateAsync("ConditionalShape"), 
            shape => 
            { 
                shape.Properties["ShouldMorph"] = false; 
                return Task.CompletedTask; 
            })
            .Location("Content")
            .Cache("conditional-morph-false", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult2.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result2 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult2.Shape));
        
        Assert.Equal("Original Conditional Content", result2.ToString());
        Assert.Equal(2, conditionalBindingCalled); // Should increase
        Assert.Equal(1, morphedBindingCalled); // Should not increase

        // Verify caching works for both cases
        for (var i = 0; i < 2; i++)
        {
            // Test cached morphed version
            shapeResult1 = new ShapeResult(
                "ConditionalShape",
                ctx => factory.CreateAsync("ConditionalShape"), 
                shape => 
                { 
                    shape.Properties["ShouldMorph"] = true; 
                    return Task.CompletedTask; 
                })
                .Location("Content")
                .Cache("conditional-morph-true", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

            contentShape = await factory.CreateAsync("Content");
            await shapeResult1.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

            var cachedResult1 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult1.Shape));
            Assert.Equal("Morphed Conditional Content", cachedResult1.ToString());

            // Test cached non-morphed version
            shapeResult2 = new ShapeResult(
                "ConditionalShape",
                ctx => factory.CreateAsync("ConditionalShape"), 
                shape => 
                { 
                    shape.Properties["ShouldMorph"] = false; 
                    return Task.CompletedTask; 
                })
                .Location("Content")
                .Cache("conditional-morph-false", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

            contentShape = await factory.CreateAsync("Content");
            await shapeResult2.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

            var cachedResult2 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult2.Shape));
            Assert.Equal("Original Conditional Content", cachedResult2.ToString());
        }

        // Counts should not have increased due to caching
        Assert.Equal(2, conditionalBindingCalled);
        Assert.Equal(1, morphedBindingCalled);
    }

    [Fact]
    public async Task ShapeMorphingWithAlternatesAndCachingPreservesAlternates()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var factory = _serviceProvider.GetService<IShapeFactory>();

        var originalBindingCalled = 0;
        var alternateBindingCalled = 0;
        var cacheTag = "alternates-morphing-test";

        // Original shape that morphs and adds alternates
        var originalDescriptor = new ShapeDescriptor { ShapeType = "OriginalShape" };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                originalBindingCalled++;
                var shape = (IShape)ctx.Value;
                
                // Preserve existing alternates and properties
                shape.Metadata.Alternates.Add("CustomAlternate");
                shape.Properties["PreservedProperty"] = "TestValue";
                
                // Morph to different type
                shape.Metadata.Type = "MorphedShape";
                shape.Metadata.Alternates.Add("MorphedShape__Special");
                
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync(shape);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        // Morphed shape with alternates
        var morphedDescriptor = new ShapeDescriptor { ShapeType = "MorphedShape" };
        morphedDescriptor.Bindings["MorphedShape"] = new ShapeBinding
        {
            BindingName = "MorphedShape",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Default Morphed Content")),
        };
        morphedDescriptor.Bindings["MorphedShape__Special"] = new ShapeBinding
        {
            BindingName = "MorphedShape__Special",
            BindingAsync = ctx =>
            {
                alternateBindingCalled++;
                var shape = (IShape)ctx.Value;
                var preservedValue = shape.Properties["PreservedProperty"];
                return Task.FromResult<IHtmlContent>(new HtmlString($"Special Morphed Content with {preservedValue}"));
            },
        };
        AddShapeDescriptor(morphedDescriptor);

        // Create cached shape result
        var shapeResult = new ShapeResult(
            "OriginalShape",
            ctx => factory.CreateAsync("OriginalShape"))
            .Location("Content")
            .Cache("alternates-morph", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        var contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        // First execution - should morph and use alternate
        var result1 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("Special Morphed Content with TestValue", result1.ToString());
        Assert.Equal(1, originalBindingCalled);
        Assert.Equal(1, alternateBindingCalled);
        Assert.Equal("MorphedShape", shapeResult.Shape.Metadata.Type);
        Assert.Contains("CustomAlternate", shapeResult.Shape.Metadata.Alternates);
        Assert.Contains("MorphedShape__Special", shapeResult.Shape.Metadata.Alternates);
        Assert.Equal("TestValue", shapeResult.Shape.Properties["PreservedProperty"]);

        // Subsequent executions should use cache
        for (var i = 0; i < 3; i++)
        {
            shapeResult = new ShapeResult(
                "OriginalShape",
                ctx => factory.CreateAsync("OriginalShape"))
                .Location("Content")
                .Cache("alternates-morph", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

            contentShape = await factory.CreateAsync("Content");
            await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

            var cachedResult = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
            Assert.Equal("Special Morphed Content with TestValue", cachedResult.ToString());
        }

        // Counts should not have increased due to caching
        Assert.Equal(1, originalBindingCalled);
        Assert.Equal(1, alternateBindingCalled);
    }

    [Fact]
    public async Task ShapeMorphingWithCacheInvalidationRecreatesCorrectly()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var factory = _serviceProvider.GetService<IShapeFactory>();
        var tagCache = _serviceProvider.GetService<ITagCache>();

        var morphingCallCount = 0;
        var finalCallCount = 0;
        var cacheTag = "invalidation-test";

        // Shape that morphs and tracks calls
        var originalDescriptor = new ShapeDescriptor { ShapeType = "InvalidationTestShape" };
        originalDescriptor.Bindings["InvalidationTestShape"] = new ShapeBinding
        {
            BindingName = "InvalidationTestShape",
            BindingAsync = async ctx =>
            {
                morphingCallCount++;
                var shape = (IShape)ctx.Value;
                shape.Metadata.Type = "FinalInvalidationShape";
                shape.Properties["CallCount"] = morphingCallCount;
                
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync(shape);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        // Final shape
        var finalDescriptor = new ShapeDescriptor { ShapeType = "FinalInvalidationShape" };
        finalDescriptor.Bindings["FinalInvalidationShape"] = new ShapeBinding
        {
            BindingName = "FinalInvalidationShape",
            BindingAsync = ctx =>
            {
                finalCallCount++;
                var shape = (IShape)ctx.Value;
                var callCount = (int)shape.Properties["CallCount"];
                return Task.FromResult<IHtmlContent>(new HtmlString($"Final Content (Call #{callCount})"));
            },
        };
        AddShapeDescriptor(finalDescriptor);

        // Helper method to create and execute shape
        async Task<(string result, IShape shape)> CreateAndExecuteShapeAsync()
        {
            var shapeResult = new ShapeResult(
                "InvalidationTestShape",
                ctx => factory.CreateAsync("InvalidationTestShape"))
                .Location("Content")
                .Cache("invalidation-test-shape", ctx => ctx.AddTag(cacheTag));

            var contentShape = await factory.CreateAsync("Content");
            await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

            var result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
            return (result.ToString(), shapeResult.Shape);
        }

        // First execution
        var (result1, shape1) = await CreateAndExecuteShapeAsync();
        Assert.Equal("Final Content (Call #1)", result1);
        Assert.Equal(1, morphingCallCount);
        Assert.Equal(1, finalCallCount);

        // Cached executions
        for (var i = 0; i < 3; i++)
        {
            var (cachedResult, cachedShape) = await CreateAndExecuteShapeAsync();
            Assert.Equal("Final Content (Call #1)", cachedResult); // Should still show call #1
            Assert.Equal(1, morphingCallCount); // Should not increase
            Assert.Equal(1, finalCallCount); // Should not increase
        }

        // Invalidate cache
        await tagCache.RemoveTagAsync(cacheTag);

        // Post-invalidation execution
        var (result2, shape2) = await CreateAndExecuteShapeAsync();
        Assert.Equal("Final Content (Call #2)", result2);
        Assert.Equal(2, morphingCallCount); // Should increase after invalidation
        Assert.Equal(2, finalCallCount); // Should increase after invalidation

        // Verify it caches again
        var (result3, shape3) = await CreateAndExecuteShapeAsync();
        Assert.Equal("Final Content (Call #2)", result3); // Should still show call #2
        Assert.Equal(2, morphingCallCount); // Should not increase
        Assert.Equal(2, finalCallCount); // Should not increase
    }

    [Fact]
    public async Task DoubleMorphingWithConcatenationCachesFinalCombinedResult()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var factory = _serviceProvider.GetService<IShapeFactory>();

        var originalBindingCalled = 0;
        var firstMorphBindingCalled = 0;
        var secondMorphBindingCalled = 0;
        var cacheTag = "double-morph-test";

        // Original shape that morphs to first shape type
        var originalDescriptor = new ShapeDescriptor { ShapeType = "DoubleMorphShape" };
        originalDescriptor.Bindings["DoubleMorphShape"] = new ShapeBinding
        {
            BindingName = "DoubleMorphShape",
            BindingAsync = async ctx =>
            {
                originalBindingCalled++;
                var shape = (IShape)ctx.Value;
                
                // Morph to first target type and execute
                shape.Metadata.Type = "FirstMorphTarget";
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                var firstResult = await displayHelper.ShapeExecuteAsync(shape);

                // Morph to second target type and execute
                shape.Metadata.Type = "SecondMorphTarget";
                shape.Metadata.ChildContent = null; // Clear ChildContent to use shape morphing
                var secondResult = await displayHelper.ShapeExecuteAsync(shape);

                // Concatenate the results
                return new HtmlString($"{firstResult}{secondResult}");
            },
        };
        AddShapeDescriptor(originalDescriptor);

        // First morph target that morphs to second target
        var firstMorphDescriptor = new ShapeDescriptor { ShapeType = "FirstMorphTarget" };
        firstMorphDescriptor.Bindings["FirstMorphTarget"] = new ShapeBinding
        {
            BindingName = "FirstMorphTarget",
            BindingAsync = ctx =>
            {
                firstMorphBindingCalled++;
                var shape = (IShape)ctx.Value;
                var sourceData = shape.Properties.TryGetValue("Data", out var data) ? data.ToString() : "NoData";
                return Task.FromResult<IHtmlContent>(new HtmlString($"[First: {sourceData}]"));
            },
        };
        AddShapeDescriptor(firstMorphDescriptor);

        // Second morph target that renders final content
        var secondMorphDescriptor = new ShapeDescriptor { ShapeType = "SecondMorphTarget" };
        secondMorphDescriptor.Bindings["SecondMorphTarget"] = new ShapeBinding
        {
            BindingName = "SecondMorphTarget",
            BindingAsync = ctx =>
            {
                secondMorphBindingCalled++;
                var shape = (IShape)ctx.Value;
                var sourceData = shape.Properties.TryGetValue("Data", out var data) ? data.ToString() : "NoData";
                return Task.FromResult<IHtmlContent>(new HtmlString($"[Second: {sourceData}]"));
            },
        };
        AddShapeDescriptor(secondMorphDescriptor);

        // Create cached shape result with data
        var shapeResult = new ShapeResult(
            "DoubleMorphShape",
            ctx => factory.CreateAsync("DoubleMorphShape"),
            shape => { shape.Properties["Data"] = "TestValue"; return Task.CompletedTask; })
            .Location("Content")
            .Cache("double-morph-concat", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        var contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        // First execution - should perform double morphing and concatenation
        var result1 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("[First: TestValue][Second: TestValue]", result1.ToString());
        Assert.Equal(1, originalBindingCalled);
        Assert.Equal(1, firstMorphBindingCalled);
        Assert.Equal(1, secondMorphBindingCalled);
        Assert.Equal("SecondMorphTarget", shapeResult.Shape.Metadata.Type); // Final morphed type

        // Second execution - should use cached result, no morphing should occur
        for (var i = 0; i < 3; i++)
        {
            shapeResult = new ShapeResult(
                "DoubleMorphShape",
                ctx => factory.CreateAsync("DoubleMorphShape"),
                shape => { shape.Properties["Data"] = "TestValue"; return Task.CompletedTask; })
                .Location("Content")
                .Cache("double-morph-concat", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

            contentShape = await factory.CreateAsync("Content");
            await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

            var cachedResult = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
            
            Assert.Equal("[First: TestValue][Second: TestValue]", cachedResult.ToString());
            Assert.Equal(1, originalBindingCalled); // Should not increase
            Assert.Equal(1, firstMorphBindingCalled); // Should not increase
            Assert.Equal(1, secondMorphBindingCalled); // Should not increase
        }

        // Test with different data to ensure cache keys work correctly
        var shapeResult2 = new ShapeResult(
            "DoubleMorphShape",
            ctx => factory.CreateAsync("DoubleMorphShape"),
            shape => { shape.Properties["Data"] = "DifferentValue"; return Task.CompletedTask; })
            .Location("Content")
            .Cache("double-morph-concat-different", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult2.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result2 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult2.Shape));
        
        Assert.Equal("[First: DifferentValue][Second: DifferentValue]", result2.ToString());
        Assert.Equal(2, originalBindingCalled); // Should increase for different cache key
        Assert.Equal(2, firstMorphBindingCalled); // Should increase for different cache key
        Assert.Equal(2, secondMorphBindingCalled); // Should increase for different cache key

        // Invalidate cache and verify morphing happens again
        var tagCache = _serviceProvider.GetService<ITagCache>();
        await tagCache.RemoveTagAsync(cacheTag);

        shapeResult = new ShapeResult(
            "DoubleMorphShape",
            ctx => factory.CreateAsync("DoubleMorphShape"),
            shape => { shape.Properties["Data"] = "TestValue"; return Task.CompletedTask; })
            .Location("Content")
            .Cache("double-morph-concat", ctx => ctx.WithExpiryAfter(TimeSpan.FromSeconds(10)).AddTag(cacheTag));

        contentShape = await factory.CreateAsync("Content");
        await shapeResult.ApplyAsync(new BuildDisplayContext(contentShape, OrchardCoreConstants.DisplayType.Detail, "", factory, null, null));

        var result3 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("[First: TestValue][Second: TestValue]", result3.ToString());
        Assert.Equal(3, originalBindingCalled); // Should increase after cache invalidation
        Assert.Equal(3, firstMorphBindingCalled); // Should increase after cache invalidation
        Assert.Equal(3, secondMorphBindingCalled); // Should increase after cache invalidation

        // Verify it caches the concatenated result again
        var result4 = await displayManager.ExecuteAsync(CreateDisplayContext(shapeResult.Shape));
        
        Assert.Equal("[First: TestValue][Second: TestValue]", result4.ToString());
        Assert.Equal(3, originalBindingCalled); // Should not increase
        Assert.Equal(3, firstMorphBindingCalled); // Should not increase
        Assert.Equal(3, secondMorphBindingCalled); // Should not increase
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
