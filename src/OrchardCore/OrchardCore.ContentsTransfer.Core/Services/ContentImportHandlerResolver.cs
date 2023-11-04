using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentsTransfer.Services;

public class ContentImportHandlerResolver : IContentImportHandlerResolver
{
    private readonly IServiceProvider _serviceProvider;
    private ContentHandlerOptions _contentHandlerOptions;

    public ContentImportHandlerResolver(
        IOptions<ContentHandlerOptions> contentHandlerOptions,
        IServiceProvider serviceProvider
        )
    {
        _contentHandlerOptions = contentHandlerOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public IList<IContentPartImportHandler> GetPartHandlers(string partName)
    {
        var services = new List<IContentPartImportHandler>();

        var handlers = _contentHandlerOptions.ContentParts
            .Where(x => x.Key.Name == partName)
            .Select(x => x.Value)
            .FirstOrDefault() ?? Enumerable.Empty<Type>();

        foreach (var handler in handlers)
        {
            services.Add((IContentPartImportHandler)_serviceProvider.GetRequiredService(handler));
        }

        return services;
    }

    public IList<IContentFieldImportHandler> GetFieldHandlers(string fieldName)
    {
        var services = new List<IContentFieldImportHandler>();

        var handlers = _contentHandlerOptions.ContentFields
            .Where(x => x.Key.Name == fieldName)
            .Select(x => x.Value)
            .FirstOrDefault() ?? Enumerable.Empty<Type>();

        foreach (var handler in handlers)
        {
            services.Add((IContentFieldImportHandler)_serviceProvider.GetRequiredService(handler));
        }

        return services;
    }
}
