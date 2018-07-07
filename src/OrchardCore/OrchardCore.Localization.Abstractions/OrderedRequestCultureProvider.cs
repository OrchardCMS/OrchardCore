using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Localization
{
    public interface IOrderedRequestCultureProvider
    {
        int Order { get; set; }
        Type RequestCultureProviderType { get; set; }
        IRequestCultureProvider RequestCultureProvider { get; set; }
    }

    public class OrderedRequestCultureProvider : IOrderedRequestCultureProvider
    {
        public OrderedRequestCultureProvider(IRequestCultureProvider requestCultureProvider, int order = 0)
        {
            Order = order;
            RequestCultureProviderType = requestCultureProvider?.GetType();
            RequestCultureProvider = requestCultureProvider;
        }

        public int Order { get; set; }
        public Type RequestCultureProviderType { get; set; }
        public IRequestCultureProvider RequestCultureProvider { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderedRequestCultureProvider(this IServiceCollection services, IRequestCultureProvider provider, int order = 0)
        {
            return services.AddSingleton<IOrderedRequestCultureProvider>(new OrderedRequestCultureProvider(provider, order));
        }

        public static IServiceCollection RemoveOrderedRequestCultureProvider(this IServiceCollection services, Type providerType)
        {
            return services.AddSingleton<IOrderedRequestCultureProvider>(new OrderedRequestCultureProvider(null) { RequestCultureProviderType = providerType });
        }
    }
}