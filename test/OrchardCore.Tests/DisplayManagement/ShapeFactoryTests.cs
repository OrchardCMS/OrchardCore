using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement;

public class ShapeFactoryTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ShapeTable _shapeTable;

    public ShapeFactoryTests()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<ILoggerFactory, NullLoggerFactory>();
        serviceCollection.AddScoped<IThemeManager, ThemeManager>();
        serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
        serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
        serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();

        _shapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        serviceCollection.AddSingleton(_shapeTable);

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task ShapeHasAttributesType()
    {
        var factory = _serviceProvider.GetService<IShapeFactory>();
        dynamic foo = await factory.CreateAsync("Foo", ArgsUtility.Empty());
        ShapeMetadata metadata = foo.Metadata;
        Assert.Equal("Foo", metadata.Type);
    }

    [Fact]
    public async Task CreateShapeWithNamedArguments()
    {
        var factory = _serviceProvider.GetService<IShapeFactory>();
        dynamic foo = await factory.CreateAsync("Foo", ArgsUtility.Named(new { one = 1, two = "dos" }));
        Assert.Equal(1, foo.one);
        Assert.Equal("dos", foo.two);
    }

    [Fact]
    public async Task CallSyntax()
    {
        dynamic factory = _serviceProvider.GetService<IShapeFactory>();
        var foo = await factory.Foo();
        ShapeMetadata metadata = foo.Metadata;
        Assert.Equal("Foo", metadata.Type);
    }

    [Fact]
    public async Task CallSyntaxAsync()
    {
        dynamic factory = _serviceProvider.GetService<IShapeFactory>();
        var foo = await factory.FooAsync();
        ShapeMetadata metadata = foo.Metadata;
        Assert.Equal("Foo", metadata.Type);
    }

    [Fact]
    public async Task CallInitializer()
    {
        dynamic factory = _serviceProvider.GetService<IShapeFactory>();
        var bar = new { One = 1, Two = "two" };
        var foo = await factory.Foo(bar);

        Assert.Equal(1, foo.One);
        Assert.Equal("two", foo.Two);
    }

    [Fact]
    public async Task ShapeFactoryUsesCustomShapeType()
    {
        var descriptor = new ShapeDescriptor
        {
            CreatingAsync = new List<Func<ShapeCreatingContext, Task>>()
            {
                (ctx) =>
                {
                    ctx.Create = () => new SubShape();
                    return Task.CompletedTask;
                },
            },
        };

        _shapeTable.Descriptors.Add("Foo", descriptor);
        dynamic factory = _serviceProvider.GetService<IShapeFactory>();
        var foo = await factory.Foo();

        Assert.IsType<SubShape>(foo);
    }

    [Fact]
    public async Task ShapeFactoryWithCustomShapeTypeAppliesArguments()
    {
        var descriptor = new ShapeDescriptor
        {
            CreatingAsync = new List<Func<ShapeCreatingContext, Task>>()
            {
                (ctx) =>
                {
                    ctx.Create = () => new SubShape();
                    return Task.CompletedTask;
                },
            },
        };

        _shapeTable.Descriptors.Add("Foo", descriptor);
        dynamic factory = _serviceProvider.GetService<IShapeFactory>();
        var foo = await factory.Foo(Bar: "Bar", Baz: "Baz");

        Assert.Equal("Bar", foo.Bar);
        Assert.Equal("Baz", foo.Baz);
    }

    [Fact]
    public async Task CreateStronglyTypedShapeUsesGeneratedShapeType()
    {
        var factory = _serviceProvider.GetRequiredService<IShapeFactory>();

        var shape = await factory.CreateAsync<TestShapeViewModel>(model =>
        {
            model.Title = "Generated";
            model.Count = 5;
        });

        var typedShape = Assert.IsAssignableFrom<TestShapeViewModel>(shape);
        var generatedShapeType = typedShape.GetType();

        Assert.NotEqual(typeof(TestShapeViewModel), generatedShapeType);
        Assert.Equal(typeof(ShapeFactoryTests).Assembly, generatedShapeType.Assembly);
        Assert.False(generatedShapeType.Assembly.IsDynamic);
        Assert.Equal("Generated", typedShape.Title);
        Assert.Equal(5, typedShape.Count);
        Assert.Same(shape.Metadata, ((IShape)shape).Metadata);
    }

    [Fact]
    public async Task CreateStronglyTypedShapeDelegatesShapeMembers()
    {
        var factory = _serviceProvider.GetRequiredService<IShapeFactory>();

        var shape = await factory.CreateAsync<TestShapeViewModel>();

        shape.Id = "shape-id";
        shape.TagName = "section";
        shape.Classes.Add("test-class");
        shape.Attributes["data-test"] = "true";
        shape.Properties["answer"] = 42;

        await shape.AddAsync(new Shape(), "1");

        var positionedShape = Assert.IsAssignableFrom<IPositioned>(shape);

        positionedShape.Position = "3";

        Assert.Equal("shape-id", shape.Id);
        Assert.Equal("section", shape.TagName);
        Assert.Contains("test-class", shape.Classes);
        Assert.Equal("true", shape.Attributes["data-test"]);
        Assert.Equal(42, shape.Properties["answer"]);
        Assert.Equal("3", positionedShape.Position);
        Assert.Single(shape.Items);
    }

    [Fact]
    public async Task CreateStronglyTypedShapeFallsBackToCastleProxy()
    {
        var factory = _serviceProvider.GetRequiredService<IShapeFactory>();
        var shapeFactoryExtensionsType = typeof(IShapeFactory).Assembly.GetType("OrchardCore.DisplayManagement.ShapeFactoryExtensions", throwOnError: true);
        var createAsyncMethod = GetCreateAsyncActionOverload(shapeFactoryExtensionsType);
        var genericCreateAsyncMethod = createAsyncMethod.MakeGenericMethod(typeof(FallbackOnlyShapeViewModel));

        Assert.NotNull(genericCreateAsyncMethod);

        var shape = await (ValueTask<IShape>)genericCreateAsyncMethod.Invoke(null, [factory, null]);
        var typedShape = Assert.IsAssignableFrom<FallbackOnlyShapeViewModel>(shape);

        Assert.NotEqual(typeof(FallbackOnlyShapeViewModel), typedShape.GetType());
        Assert.True(typedShape.GetType().Assembly.IsDynamic);
    }

    [Fact]
    public async Task DisplayDriverInitializeUsesGeneratedShapeType()
    {
        var factory = _serviceProvider.GetRequiredService<IShapeFactory>();
        var shapeResult = new TestDisplayDriver().Build();
        var shape = await BuildShapeAsync(shapeResult, factory);
        var typedShape = Assert.IsAssignableFrom<TestShapeViewModel>(shape);
        var generatedShapeType = typedShape.GetType();

        Assert.NotEqual(typeof(TestShapeViewModel), generatedShapeType);
        Assert.Equal(typeof(ShapeFactoryTests).Assembly, generatedShapeType.Assembly);
        Assert.False(generatedShapeType.Assembly.IsDynamic);
        Assert.Equal("Driver", typedShape.Title);
        Assert.Equal(10, typedShape.Count);
    }

    private static MethodInfo GetCreateAsyncActionOverload(Type shapeFactoryExtensionsType)
        => shapeFactoryExtensionsType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == "CreateAsync" &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1 &&
                method.GetParameters().Length == 2 &&
                method.GetParameters()[0].ParameterType == typeof(IShapeFactory) &&
                method.GetParameters()[1].ParameterType.IsGenericType &&
                method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

    private static async Task<IShape> BuildShapeAsync(ShapeResult shapeResult, IShapeFactory factory)
    {
        var shapeBuilderField = typeof(ShapeResult).GetField("_shapeBuilder", BindingFlags.Instance | BindingFlags.NonPublic);
        var initializingAsyncField = typeof(ShapeResult).GetField("_initializingAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(shapeBuilderField);
        Assert.NotNull(initializingAsyncField);

        var shapeBuilder = Assert.IsType<Func<IBuildShapeContext, ValueTask<IShape>>>(shapeBuilderField.GetValue(shapeResult));
        var initializingAsync = (Func<IShape, Task>)initializingAsyncField.GetValue(shapeResult);
        var shape = await shapeBuilder(new BuildDisplayContext(new Shape(), "Detail", string.Empty, factory, null, null));

        if (initializingAsync is not null)
        {
            await initializingAsync(shape);
        }

        return shape;
    }

    private sealed class SubShape : Shape
    {
    }

    private sealed class TestDisplayDriver : DisplayDriverBase
    {
        public ShapeResult Build()
            => Initialize<TestShapeViewModel>("TestShapeViewModel_Edit", async model =>
            {
                model.Title = "Driver";
                model.Count = 10;
                await ValueTask.CompletedTask;
            });
    }
}

public class TestShapeViewModel
{
    public string Title { get; set; }

    public int Count { get; set; }
}

public class FallbackOnlyShapeViewModel
{
    public string Name { get; set; }
}
