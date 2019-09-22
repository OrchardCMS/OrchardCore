using System;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverFactory
    {
        IContentPartDisplayDriver GetDisplayDriver(string partName);
    }

    public class ContentPartDisplayDriverFactory : IContentPartDisplayDriverFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ContentOptions _contentOptions;
        public ContentPartDisplayDriverFactory(
            IServiceProvider serviceProvider,
            IOptions<ContentOptions> contentOptions
            )
        {
            _serviceProvider = serviceProvider;
            _contentOptions = contentOptions.Value;
        }

        public IContentPartDisplayDriver GetDisplayDriver(string partName)
        {
            if (_contentOptions.ContentPartOptionsLookup.TryGetValue(partName, out var displayTypeDriver))
            {
                if (displayTypeDriver.FactoryTypes.TryGetValue("displaydriver", out var factory))
                {
                    var service = _serviceProvider.GetService(factory) as IContentPartDisplayDriver;
                    return service;
                }
            }
            return null;
        }
    }
}
