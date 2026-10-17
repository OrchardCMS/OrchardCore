using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Describes a registered <see cref="ISharedLinkHandler"/> with the static members of its class, so the kinds of the
/// links are known without creating their handlers. <c>services.AddSharedLinkHandler&lt;THandler&gt;()</c> registers
/// one per handler.
/// </summary>
public sealed class SharedLinkHandlerDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SharedLinkHandlerDescriptor"/> class.
    /// </summary>
    /// <param name="kind">The kind of resource the handler opens.</param>
    /// <param name="displayName">The name of the kind of resource, shown in the admin.</param>
    /// <param name="type">The class of the handler, resolved from the services when a link is opened or deleted.</param>
    public SharedLinkHandlerDescriptor(string kind, LocalizationSource displayName, Type type)
    {
        ArgumentException.ThrowIfNullOrEmpty(kind);
        ArgumentNullException.ThrowIfNull(type);

        Kind = kind;
        DisplayName = displayName;
        Type = type;
    }

    /// <summary>
    /// Gets the kind of resource the handler opens, such as <c>DataPipelines.File</c>.
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// Gets the name of the kind of resource, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the class of the handler, which implements <see cref="ISharedLinkHandler"/>.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Describes a handler from the static members of its class.
    /// </summary>
    /// <typeparam name="THandler">The class of the handler.</typeparam>
    /// <returns>The descriptor.</returns>
    public static SharedLinkHandlerDescriptor Create<THandler>()
        where THandler : class, ISharedLinkHandler
    {
        return new SharedLinkHandlerDescriptor(THandler.Kind, THandler.DisplayName, typeof(THandler));
    }
}

/// <summary>
/// Holds the registered handlers of shared links. <c>services.AddSharedLinkHandler&lt;THandler&gt;()</c> adds them.
/// </summary>
public sealed class SharedLinkHandlerOptions
{
    private readonly List<SharedLinkHandlerDescriptor> _handlers = [];

    /// <summary>
    /// Gets the registered handlers, in the order they were registered.
    /// </summary>
    public IReadOnlyList<SharedLinkHandlerDescriptor> Handlers => _handlers;

    /// <summary>
    /// Registers a handler, unless its class is already registered.
    /// </summary>
    /// <param name="descriptor">The handler.</param>
    /// <returns>The options.</returns>
    public SharedLinkHandlerOptions Add(SharedLinkHandlerDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (!_handlers.Any(handler => handler.Type == descriptor.Type))
        {
            _handlers.Add(descriptor);
        }

        return this;
    }
}
