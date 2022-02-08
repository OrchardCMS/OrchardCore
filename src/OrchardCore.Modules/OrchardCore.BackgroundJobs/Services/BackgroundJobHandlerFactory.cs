using System;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobHandlerFactory : IBackgroundJobHandlerFactory
    {
        private readonly BackgroundJobOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public BackgroundJobHandlerFactory(IOptions<BackgroundJobOptions> options, IServiceProvider serviceProvider)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        public IBackgroundJobHandler Create(string name)
        {
            if (_options.BackgroundJobs.TryGetValue(name, out var jobOption))
            {
                return (IBackgroundJobHandler)jobOption.HandlerFactory(_serviceProvider, arguments: null);
            }

            return null;
        }
    }
}
