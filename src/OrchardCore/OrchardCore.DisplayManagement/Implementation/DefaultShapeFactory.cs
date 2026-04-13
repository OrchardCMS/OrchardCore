using System.Dynamic;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.DisplayManagement.Implementation;

public class DefaultShapeFactory : DynamicObject, IShapeFactory
{
    private readonly IEnumerable<IShapeFactoryEvents> _events;
    private readonly IShapeTableManager _shapeTableManager;
    private readonly IThemeManager _themeManager;
    private readonly IServiceProvider _serviceProvider;

    public DefaultShapeFactory(
        IEnumerable<IShapeFactoryEvents> events,
        IShapeTableManager shapeTableManager,
        IThemeManager themeManager,
        IServiceProvider serviceProvider)
    {
        _events = events;
        _shapeTableManager = shapeTableManager;
        _themeManager = themeManager;
        _serviceProvider = serviceProvider;
    }

    public dynamic New => this;

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        // await New.FooAsync()
        // await New.Foo()

        var binderName = binder.Name;

        if (binderName.EndsWith("Async", StringComparison.Ordinal))
        {
            binderName = binder.Name[..^"Async".Length];
        }

        result = ShapeFactoryExtensions
            .CreateAsync(this, binderName, Arguments.From(args, binder.CallInfo.ArgumentNames))
            .AsTask();

        return true;
    }

    public ValueTask<IShape> CreateAsync(
        string shapeType,
        Func<ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext> creating,
        Action<ShapeCreatedContext> created)
        => CreateAsync(
            shapeType,
            static state => state.shapeFactory(),
            static (ctx, state) => state.creating?.Invoke(ctx),
            static (ctx, state) => state.created?.Invoke(ctx),
            (shapeFactory, creating, created));

    public async ValueTask<IShape> CreateAsync<TState>(
        string shapeType,
        Func<TState, ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext, TState> creating,
        Action<ShapeCreatedContext, TState> created,
        TState state)
    {
        ShapeDescriptor shapeDescriptor;
        (await _themeManager.GetShapeTableAsync(_shapeTableManager)).Descriptors.TryGetValue(shapeType, out shapeDescriptor);

        var creatingContext = new ShapeCreatingContext<TState>
        {
            ServiceProvider = _serviceProvider,
            New = this,
            ShapeFactory = this,
            ShapeType = shapeType,
            CreateAsyncWithState = shapeFactory,
            State = state,
        };

        creating?.Invoke(creatingContext, state);

        // 'Creating' events may add behaviors and alter base type.
        foreach (var ev in _events)
        {
            ev.Creating(creatingContext);
        }

        if (shapeDescriptor != null)
        {
            foreach (var ev in shapeDescriptor.CreatingAsync)
            {
                await ev(creatingContext);
            }
        }

        // Create the new instance.
        var shape = await creatingContext.CreateInternalAsync()
            ?? throw new InvalidOperationException($"Shape creation failed for type '{shapeType}'. The shape factory returned null.");

        var createdContext = new ShapeCreatedContext
        {
            ServiceProvider = _serviceProvider,
            New = creatingContext.New,
            ShapeFactory = creatingContext.ShapeFactory,
            ShapeType = creatingContext.ShapeType,
            Shape = shape,
        };

        var shapeMetadata = shape.Metadata;
        shapeMetadata.Type = shapeType;

        // Merge wrappers if there are any.
        if (shapeDescriptor != null && shapeMetadata.Wrappers.Count + shapeDescriptor.Wrappers.Count > 0)
        {
            shapeMetadata.Wrappers.AddRange(shapeDescriptor.Wrappers);
        }

        // 'Created' events provides default values and new object initialization.
        foreach (var ev in _events)
        {
            ev.Created(createdContext);
        }

        if (shapeDescriptor != null)
        {
            foreach (var ev in shapeDescriptor.CreatedAsync)
            {
                await ev(createdContext);
            }
        }

        if (creatingContext.HasOnCreated)
        {
            foreach (var ev in creatingContext.OnCreated)
            {
                await ev(createdContext);
            }
        }

        created?.Invoke(createdContext, state);

        return createdContext.Shape;
    }
}
