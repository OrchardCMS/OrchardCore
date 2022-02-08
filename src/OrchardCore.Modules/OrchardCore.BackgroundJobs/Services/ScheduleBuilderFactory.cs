using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs.Schedules;

namespace OrchardCore.BackgroundJobs.Services
{
    public class ScheduleBuilderFactory : IScheduleBuilderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ScheduleBuilderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public T Create<T>() where T : IScheduleBuilder
            => Cache<T>.Create(_serviceProvider);

        private static class Cache<IScheduleBuilder>
        {
            private static readonly ObjectFactory _objectFactory = ActivatorUtilities.CreateFactory(typeof(IScheduleBuilder), Type.EmptyTypes);

            public static IScheduleBuilder Create(IServiceProvider serviceProvider) => (IScheduleBuilder)_objectFactory(serviceProvider, arguments: null);
        }
    }
}
