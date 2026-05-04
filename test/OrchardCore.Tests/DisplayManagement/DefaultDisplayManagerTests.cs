using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Localization;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement;

public class DefaultDisplayManagerTests
{
    private readonly ShapeTable _defaultShapeTable;
    private readonly TestShapeBindingsDictionary _additionalBindings;
    private readonly IServiceProvider _serviceProvider;

    public DefaultDisplayManagerTests()
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
        serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
        serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();
        serviceCollection.AddScoped<IShapeBindingResolver, TestShapeBindingResolver>();
        serviceCollection.AddScoped<IShapeDisplayEvents, TestDisplayEvents>();
        serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
        serviceCollection.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
        serviceCollection.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

        serviceCollection.AddLogging();

        serviceCollection.AddSingleton(_defaultShapeTable);
        serviceCollection.AddSingleton(_additionalBindings);
        serviceCollection.AddWebEncoders();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private sealed class TestDisplayEvents : IShapeDisplayEvents
    {
        public Action<ShapeDisplayContext> Displaying = ctx => { };
        public Action<ShapeDisplayContext> Displayed = ctx => { };
        public Action<ShapeDisplayContext> Finalized = ctx => { };

        Task IShapeDisplayEvents.DisplayingAsync(ShapeDisplayContext context)
        {
            Displaying(context); return Task.CompletedTask;
        }
        Task IShapeDisplayEvents.DisplayedAsync(ShapeDisplayContext context)
        {
            Displayed(context); return Task.CompletedTask;
        }
        Task IShapeDisplayEvents.DisplayingFinalizedAsync(ShapeDisplayContext context)
        {
            Finalized(context); return Task.CompletedTask;
        }
    }

    private void AddShapeDescriptor(ShapeDescriptor shapeDescriptor)
    {
        _defaultShapeTable.Descriptors[shapeDescriptor.ShapeType] = shapeDescriptor;
        foreach (var binding in shapeDescriptor.Bindings)
        {
            _defaultShapeTable.Bindings[binding.Key] = binding.Value;
        }
    }

    private DisplayContext CreateDisplayContext(Shape shape)
    {
        return new DisplayContext
        {
            Value = shape,
            ServiceProvider = _serviceProvider,
        };
    }

    [Fact]
    public async Task RenderSimpleShape()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
        };
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Hi there!", result.ToString());
    }

    [Fact]
    public async Task RenderIShapeBindingResolverProvidedShapes()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Baz";

        _additionalBindings["Baz"] = new ShapeBinding
        {
            BindingName = "Baz",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi from IShapeBindingResolver.")),
        };

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        // Cleanup
        _additionalBindings.Clear();

        Assert.Equal("Hi from IShapeBindingResolver.", result.ToString());
    }

    [Fact]
    public async Task RenderPreCalculatedShape()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";
        shape.Metadata.OnDisplaying(
            context =>
            {
                context.ChildContent = new HtmlString("Bar");
            });

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
        };
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Bar", result.ToString());
    }

    [Fact]
    public async Task IShapeBindingResolverProvidedShapesDoesNotOverrideShapeDescriptor()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
            ProcessingAsync = new Func<ShapeDisplayContext, Task>[] {
                context =>
                {
                    dynamic dynamicShape = context.Shape;
                    dynamicShape.Data = "some data";
                    return Task.CompletedTask;
                },
            },
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Is there any data ?")),
        };
        AddShapeDescriptor(descriptor);

        _additionalBindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString($"Yes there is {((dynamic)ctx.Value).Data}.")),
        };

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        // Cleanup
        _additionalBindings.Clear();

        Assert.Equal("Yes there is some data.", result.ToString());
    }

    [Fact]
    public async Task RenderFallbackShape()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo__2";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
        };
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Hi there!", result.ToString());
    }

    [Fact]
    public async Task AddAlternatesOnDisplaying()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
            DisplayingAsync = new Func<ShapeDisplayContext, Task>[] {
                context =>
                {
                        context.Shape.Metadata.Alternates.Add("Bar");
                        return Task.CompletedTask;
                },
            },
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Foo")),
        };
        descriptor.Bindings["Bar"] = new ShapeBinding
        {
            BindingName = "Bar",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Bar")),
        };
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Bar", result.ToString());
    }

    [Fact]
    public async Task AddAlternatesOnProcessing()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
            ProcessingAsync = new Func<ShapeDisplayContext, Task>[] {
                context =>
                {
                        context.Shape.Metadata.Alternates.Add("Bar");
                        return Task.CompletedTask;
                },
            },
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Foo")),
        };
        descriptor.Bindings["Bar"] = new ShapeBinding
        {
            BindingName = "Bar",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Bar")),
        };
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Bar", result.ToString());
    }

    [Fact]
    public async Task RenderAlternateShapeExplicitly()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo__2";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        descriptor.Bindings["Foo"] = new ShapeBinding
        {
            BindingName = "Foo",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")),
        };
        descriptor.Bindings["Foo__2"] = new ShapeBinding
        {
            BindingName = "Foo__2",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hello again!")),
        };
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Hello again!", result.ToString());
    }

    [Fact]
    public async Task RenderAlternateShapeByMostRecentlyAddedMatchingAlternate()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";
        shape.Metadata.Alternates.Add("Foo__1");
        shape.Metadata.Alternates.Add("Foo__2");
        shape.Metadata.Alternates.Add("Foo__3");

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        AddBinding(descriptor, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hi there!")));
        AddBinding(descriptor, "Foo__1", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hello (1)!")));
        AddBinding(descriptor, "Foo__2", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Hello (2)!")));
        AddShapeDescriptor(descriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Hello (2)!", result.ToString());
    }

    private static void AddBinding(ShapeDescriptor descriptor, string bindingName, Func<DisplayContext, Task<IHtmlContent>> binding)
    {
        descriptor.Bindings[bindingName] = new ShapeBinding
        {
            BindingName = bindingName,
            BindingAsync = binding,
        };
    }

    [Fact]
    public async Task ShapeDescriptorDisplayingAndDisplayedAreCalled()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "Foo";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        AddBinding(descriptor, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("yarg")));
        AddShapeDescriptor(descriptor);

        var displayingEventCount = 0;
        var displayedEventCount = 0;
        descriptor.DisplayingAsync = new Func<ShapeDisplayContext, Task>[] { ctx => { ++displayingEventCount; return Task.CompletedTask; } };
        descriptor.DisplayedAsync = new Func<ShapeDisplayContext, Task>[] { ctx => { ++displayedEventCount; ctx.ChildContent = new HtmlString("[" + ctx.ChildContent.ToString() + "]"); return Task.CompletedTask; } };

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal(1, displayingEventCount);
        Assert.Equal(1, displayedEventCount);
        Assert.Equal("[yarg]", result.ToString());
    }

    [Fact]
    public async Task DisplayingEventFiresEarlyEnoughToAddAlternateShapeBindingNames()
    {
        var htmlDisplay = _serviceProvider.GetService<IHtmlDisplay>();

        var shapeFoo = new Shape();
        shapeFoo.Metadata.Type = "Foo";

        var descriptorFoo = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        AddBinding(descriptorFoo, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("alpha")));
        AddShapeDescriptor(descriptorFoo);

        var descriptorBar = new ShapeDescriptor
        {
            ShapeType = "Bar",
        };
        AddBinding(descriptorBar, "Bar", ctx => Task.FromResult<IHtmlContent>(new HtmlString("beta")));
        AddShapeDescriptor(descriptorBar);

        var resultNormally = await htmlDisplay.ExecuteAsync(CreateDisplayContext(shapeFoo));

        shapeFoo = new Shape();
        shapeFoo.Metadata.Type = "Foo";
        descriptorFoo.DisplayingAsync = new Func<ShapeDisplayContext, Task>[] { ctx => { ctx.Shape.Metadata.Alternates.Add("Bar"); return Task.CompletedTask; } };
        var resultWithOverride = await htmlDisplay.ExecuteAsync(CreateDisplayContext(shapeFoo));

        Assert.Equal("alpha", resultNormally.ToString());
        Assert.Equal("beta", resultWithOverride.ToString());
    }

    [Fact]
    public async Task ShapeTypeAndBindingNamesAreNotCaseSensitive()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shapeFoo = new Shape();
        shapeFoo.Metadata.Type = "Foo";

        var descriptorFoo = new ShapeDescriptor
        {
            ShapeType = "Foo",
        };
        AddBinding(descriptorFoo, "Foo", ctx => Task.FromResult<IHtmlContent>(new HtmlString("alpha")));
        AddShapeDescriptor(descriptorFoo);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shapeFoo));

        Assert.Equal("alpha", result.ToString());
    }

    [Fact]
    public async Task IShapeDisplayEventsCalledInCorrectOrder()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var testEvents = _serviceProvider.GetService<IShapeDisplayEvents>() as TestDisplayEvents;

        var shape = new Shape();
        shape.Metadata.Type = "OrderTest";

        var descriptor = new ShapeDescriptor
        {
            ShapeType = "OrderTest",
        };
        AddBinding(descriptor, "OrderTest", ctx => Task.FromResult<IHtmlContent>(new HtmlString("Order Test Content")));
        AddShapeDescriptor(descriptor);

        var eventOrder = new List<string>();

        // Override the event handlers to track the order of calls
        testEvents.Displaying = ctx => eventOrder.Add("Displaying");
        testEvents.Displayed = ctx => eventOrder.Add("Displayed");
        testEvents.Finalized = ctx => eventOrder.Add("Finalized");

        await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        // Verify that events were called in the correct order
        Assert.Equal(3, eventOrder.Count);
        Assert.Equal("Displaying", eventOrder[0]);
        Assert.Equal("Displayed", eventOrder[1]);
        Assert.Equal("Finalized", eventOrder[2]);
    }

    [Fact]
    public async Task ShapeMorphingChangesTypeAndUsesNewBinding()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "OriginalShape";

        var originalDescriptor = new ShapeDescriptor
        {
            ShapeType = "OriginalShape",
        };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                // Morph the shape to a different type
                ((IShape)ctx.Value).Metadata.Type = "MorphedShape";

                // Re-execute with the new type using IDisplayHelper
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        var morphedDescriptor = new ShapeDescriptor
        {
            ShapeType = "MorphedShape",
        };
        morphedDescriptor.Bindings["MorphedShape"] = new ShapeBinding
        {
            BindingName = "MorphedShape",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Morphed Content")),
        };
        AddShapeDescriptor(morphedDescriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal("Morphed Content", result.ToString());
        Assert.Equal("MorphedShape", shape.Metadata.Type);
    }

    [Fact]
    public async Task ShapeMorphingWithAlternatePreservesOriginalMetadata()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "OriginalShape";
        shape.Metadata.Alternates.Add("CustomAlternate");
        shape.Properties["CustomProperty"] = "TestValue";

        var originalDescriptor = new ShapeDescriptor
        {
            ShapeType = "OriginalShape",
        };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                var originalAlternates = ((IShape)ctx.Value).Metadata.Alternates.ToList();
                var originalProperties = ((IShape)ctx.Value).Properties.ToDictionary(p => p.Key, p => p.Value);

                // Morph the shape to a different type
                ((IShape)ctx.Value).Metadata.Type = "MorphedShape";

                // Re-execute with the new type using IDisplayHelper
                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                var result = await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);

                // Verify metadata preservation
                Assert.Contains("CustomAlternate", ((IShape)ctx.Value).Metadata.Alternates);
                Assert.Equal("TestValue", ((IShape)ctx.Value).Properties["CustomProperty"]);

                return result;
            },
        };
        AddShapeDescriptor(originalDescriptor);

        var morphedDescriptor = new ShapeDescriptor
        {
            ShapeType = "MorphedShape",
        };
        morphedDescriptor.Bindings["MorphedShape"] = new ShapeBinding
        {
            BindingName = "MorphedShape",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Morphed with Metadata")),
        };
        AddShapeDescriptor(morphedDescriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal("Morphed with Metadata", result.ToString());
        Assert.Equal("MorphedShape", shape.Metadata.Type);
        Assert.Contains("CustomAlternate", shape.Metadata.Alternates);
        Assert.Equal("TestValue", shape.Properties["CustomProperty"]);
    }

    [Fact]
    public async Task ShapeMorphingWithConditionalLogic()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "ConditionalShape";
        shape.Properties["ShouldMorph"] = true;

        var conditionalDescriptor = new ShapeDescriptor
        {
            ShapeType = "ConditionalShape",
        };
        conditionalDescriptor.Bindings["ConditionalShape"] = new ShapeBinding
        {
            BindingName = "ConditionalShape",
            BindingAsync = async ctx =>
            {
                var shouldMorph = ((IShape)ctx.Value).Properties.TryGetValue("ShouldMorph", out var morphValue) && (bool)morphValue;

                if (shouldMorph)
                {
                    // Morph the shape
                    ((IShape)ctx.Value).Metadata.Type = "MorphedConditionalShape";

                    var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                    return await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);
                }

                return new HtmlString("Original Conditional Content");
            },
        };
        AddShapeDescriptor(conditionalDescriptor);

        var morphedDescriptor = new ShapeDescriptor
        {
            ShapeType = "MorphedConditionalShape",
        };
        morphedDescriptor.Bindings["MorphedConditionalShape"] = new ShapeBinding
        {
            BindingName = "MorphedConditionalShape",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Morphed Conditional Content")),
        };
        AddShapeDescriptor(morphedDescriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal("Morphed Conditional Content", result.ToString());

        // Test the non-morphing condition
        shape.Metadata.Type = "ConditionalShape"; // Reset type
        shape.Properties["ShouldMorph"] = false;
        shape.Metadata.ChildContent = null; // Clear any previous content

        var result2 = await displayManager.ExecuteAsync(CreateDisplayContext(shape));
        Assert.Equal("Original Conditional Content", result2.ToString());
    }

    [Fact]
    public async Task ShapeMorphingWithAlternateBinding()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "OriginalShape";

        var originalDescriptor = new ShapeDescriptor
        {
            ShapeType = "OriginalShape",
        };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                // Morph to a shape type that has alternates
                ((IShape)ctx.Value).Metadata.Type = "MorphedShape";
                ((IShape)ctx.Value).Metadata.Alternates.Add("MorphedShape__Special");

                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        var morphedDescriptor = new ShapeDescriptor
        {
            ShapeType = "MorphedShape",
        };
        morphedDescriptor.Bindings["MorphedShape"] = new ShapeBinding
        {
            BindingName = "MorphedShape",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Default Morphed Content")),
        };
        morphedDescriptor.Bindings["MorphedShape__Special"] = new ShapeBinding
        {
            BindingName = "MorphedShape__Special",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Special Morphed Content")),
        };
        AddShapeDescriptor(morphedDescriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal("Special Morphed Content", result.ToString());
        Assert.Equal("MorphedShape", shape.Metadata.Type);
        Assert.Contains("MorphedShape__Special", shape.Metadata.Alternates);
    }

    [Fact]
    public async Task ShapeMorphingChain()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "FirstShape";
        shape.Properties["MorphCount"] = 0;

        // First shape that morphs to second
        var firstDescriptor = new ShapeDescriptor
        {
            ShapeType = "FirstShape",
        };
        firstDescriptor.Bindings["FirstShape"] = new ShapeBinding
        {
            BindingName = "FirstShape",
            BindingAsync = async ctx =>
            {
                var shapeInstance = (IShape)ctx.Value;
                var morphCount = (int)shapeInstance.Properties["MorphCount"];
                shapeInstance.Properties["MorphCount"] = morphCount + 1;

                // Morph to second shape
                shapeInstance.Metadata.Type = "SecondShape";

                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync(shapeInstance);
            },
        };
        AddShapeDescriptor(firstDescriptor);

        // Second shape that morphs to third
        var secondDescriptor = new ShapeDescriptor
        {
            ShapeType = "SecondShape",
        };
        secondDescriptor.Bindings["SecondShape"] = new ShapeBinding
        {
            BindingName = "SecondShape",
            BindingAsync = async ctx =>
            {
                var shapeInstance = (IShape)ctx.Value;
                var morphCount = (int)shapeInstance.Properties["MorphCount"];
                shapeInstance.Properties["MorphCount"] = morphCount + 1;

                // Morph to third shape
                shapeInstance.Metadata.Type = "ThirdShape";

                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync(shapeInstance);
            },
        };
        AddShapeDescriptor(secondDescriptor);

        // Final shape that renders content
        var thirdDescriptor = new ShapeDescriptor
        {
            ShapeType = "ThirdShape",
        };
        thirdDescriptor.Bindings["ThirdShape"] = new ShapeBinding
        {
            BindingName = "ThirdShape",
            BindingAsync = ctx =>
            {
                var shapeInstance = (IShape)ctx.Value;
                var morphCount = (int)shapeInstance.Properties["MorphCount"];
                return Task.FromResult<IHtmlContent>(new HtmlString($"Final Content (Morphed {morphCount} times)"));
            },
        };
        AddShapeDescriptor(thirdDescriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal("Final Content (Morphed 2 times)", result.ToString());
        Assert.Equal("ThirdShape", shape.Metadata.Type);
        Assert.Equal(2, shape.Properties["MorphCount"]);
    }

    [Fact]
    public async Task ShapeMorphingWithDisplayEvents()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();
        var testEvents = _serviceProvider.GetService<IShapeDisplayEvents>() as TestDisplayEvents;

        var shape = new Shape();
        shape.Metadata.Type = "OriginalShape";

        var eventLog = new List<string>();

        testEvents.Displaying = ctx => eventLog.Add($"Displaying: {ctx.Shape.Metadata.Type}");
        testEvents.Displayed = ctx => eventLog.Add($"Displayed: {ctx.Shape.Metadata.Type}");
        testEvents.Finalized = ctx => eventLog.Add($"Finalized: {ctx.Shape.Metadata.Type}");

        var originalDescriptor = new ShapeDescriptor
        {
            ShapeType = "OriginalShape",
        };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                // Morph the shape
                ((IShape)ctx.Value).Metadata.Type = "MorphedShape";

                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        var morphedDescriptor = new ShapeDescriptor
        {
            ShapeType = "MorphedShape",
        };
        morphedDescriptor.Bindings["MorphedShape"] = new ShapeBinding
        {
            BindingName = "MorphedShape",
            BindingAsync = ctx => Task.FromResult<IHtmlContent>(new HtmlString("Morphed Content")),
        };
        AddShapeDescriptor(morphedDescriptor);

        var result = await displayManager.ExecuteAsync(CreateDisplayContext(shape));

        Assert.Equal("Morphed Content", result.ToString());

        // Verify events were called for both original and morphed shapes
        Assert.Contains("Displaying: OriginalShape", eventLog);
        Assert.Contains("Displaying: MorphedShape", eventLog);
        Assert.Contains("Displayed: MorphedShape", eventLog);
        Assert.Contains("Finalized: MorphedShape", eventLog);
        // Note: The Finalized event for the original shape is called, but the shape type
        // has already been changed to MorphedShape, so it won't log as OriginalShape.
    }

    [Fact]
    public async Task ShapeMorphingFailsWhenTargetShapeNotFound()
    {
        var displayManager = _serviceProvider.GetService<IHtmlDisplay>();

        var shape = new Shape();
        shape.Metadata.Type = "OriginalShape";

        var originalDescriptor = new ShapeDescriptor
        {
            ShapeType = "OriginalShape",
        };
        originalDescriptor.Bindings["OriginalShape"] = new ShapeBinding
        {
            BindingName = "OriginalShape",
            BindingAsync = async ctx =>
            {
                // Morph to a non-existent shape type
                ((IShape)ctx.Value).Metadata.Type = "NonExistentShape";

                var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
                return await displayHelper.ShapeExecuteAsync((IShape)ctx.Value);
            },
        };
        AddShapeDescriptor(originalDescriptor);

        await Assert.ThrowsAsync<InvalidOperationException>(() => displayManager.ExecuteAsync(CreateDisplayContext(shape)));
    }
}
