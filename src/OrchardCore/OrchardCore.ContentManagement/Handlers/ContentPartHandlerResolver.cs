using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Handlers;

public class ContentPartHandlerResolver : IContentPartHandlerResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ContentOptions _contentOptions;

    // The handlers are scoped services, they are resolved once per scope as they are requested for each content item.
    private readonly Dictionary<string, IContentPartHandler[]> _handlers = [];

    public ContentPartHandlerResolver(
        IServiceProvider serviceProvider,
        IOptions<ContentOptions> contentDisplayOptions
        )
    {
        _serviceProvider = serviceProvider;
        _contentOptions = contentDisplayOptions.Value;
    }

    public IList<IContentPartHandler> GetHandlers(string partName)
    {
        if (_handlers.TryGetValue(partName, out var handlers))
        {
            return handlers;
        }

        if (_contentOptions.ContentPartOptionsLookup.TryGetValue(partName, out var contentPartOption) && contentPartOption.Handlers.Count > 0)
        {
            handlers = new IContentPartHandler[contentPartOption.Handlers.Count];

            for (var i = 0; i < handlers.Length; i++)
            {
                handlers[i] = (IContentPartHandler)_serviceProvider.GetRequiredService(contentPartOption.Handlers[i]);
            }
        }
        else
        {
            handlers = [];
        }

        _handlers[partName] = handlers;

        return handlers;
    }
}
