using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Handlers;

public class ContentFieldHandlerResolver : IContentFieldHandlerResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ContentOptions _contentOptions;

    // The handlers are scoped services, they are resolved once per scope as they are requested for each content item.
    private readonly Dictionary<string, List<IContentFieldHandler>> _handlers = [];

    public ContentFieldHandlerResolver(
        IServiceProvider serviceProvider,
        IOptions<ContentOptions> contentDisplayOptions
        )
    {
        _serviceProvider = serviceProvider;
        _contentOptions = contentDisplayOptions.Value;
    }

    public IList<IContentFieldHandler> GetHandlers(string fieldName)
    {
        if (_handlers.TryGetValue(fieldName, out var handlers))
        {
            return handlers;
        }

        handlers = [];

        if (_contentOptions.ContentFieldOptionsLookup.TryGetValue(fieldName, out var contentFieldOption) && contentFieldOption.Handlers.Count > 0)
        {
            for (var i = 0; i < contentFieldOption.Handlers.Count; i++)
            {
                handlers.Add((IContentFieldHandler)_serviceProvider.GetRequiredService(contentFieldOption.Handlers[i]));
            }
        }

        _handlers[fieldName] = handlers;

        return handlers;
    }
}
