using System;

namespace OrchardCore.Filters.Abstractions.Services
{
    public class FilterExecutionContext<T>
    {
        public FilterExecutionContext(T item, IServiceProvider serviceProvider)
        {
            Item = item;
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
        public T Item { get; }
    }
}
