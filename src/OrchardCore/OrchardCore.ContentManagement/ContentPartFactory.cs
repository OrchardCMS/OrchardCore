using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement;

/// <summary>
/// Implements <see cref="ITypeActivatorFactory{ContentPart}"/> by resolving all registered <see cref="ContentPart"/> types
/// and memorizing a statically typed <see cref="ITypeActivator{ContentPart}"/>.
/// </summary>
public class ContentPartFactory : ITypeActivatorFactory<ContentPart>
{
    private readonly ITypeActivator<ContentPart> _contentPartActivator = new GenericTypeActivator<ContentPart, ContentPart>();

    private readonly Dictionary<string, ITypeActivator<ContentPart>> _contentPartActivators;

    public ContentPartFactory(IOptions<ContentOptions> contentOptions, ILogger<ContentPartFactory> logger)
    {
        _contentPartActivators = [];

        // Check content options for configured parts.
        foreach (var partOption in contentOptions.Value.ContentPartOptions)
        {
            if (_contentPartActivators.ContainsKey(partOption.Type.Name))
            {
                logger.LogWarning("The ContentPart '{Name}' was registered more than once. Content Parts should only be registered once using .AddContentPart<{Name}>().", partOption.Type.Name, partOption.Type.Name);

                continue;
            }

            var activatorType = typeof(GenericTypeActivator<,>).MakeGenericType(partOption.Type, typeof(ContentPart));
            var activator = (ITypeActivator<ContentPart>)Activator.CreateInstance(activatorType);

            _contentPartActivators.Add(partOption.Type.Name, activator);
        }
    }

    /// <inheritdoc />
    public ITypeActivator<ContentPart> GetTypeActivator(string partName)
    {
        if (_contentPartActivators.TryGetValue(partName, out var activator))
        {
            return activator;
        }

        return _contentPartActivator;
    }
}
