using System;
using System.Linq;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Localization
{
    public interface IOrderedRequestCultureProvider
    {
        int Order { get; set; }
        IRequestCultureProvider RequestCultureProvider { get; }
    }

    public class OrderedRequestCultureProvider : IOrderedRequestCultureProvider
    {
        public OrderedRequestCultureProvider(IRequestCultureProvider requestCultureProvider, int order = 0)
        {
            Order = order;
            RequestCultureProvider = requestCultureProvider;
        }

        public int Order { get; set; }
        public IRequestCultureProvider RequestCultureProvider { get; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrUpdateOrderedRequestCultureProvider(this IServiceCollection services, IRequestCultureProvider provider, int order = 0)
        {
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IOrderedRequestCultureProvider) &&
                ((IOrderedRequestCultureProvider)s.ImplementationInstance).RequestCultureProvider.GetType() == provider.GetType());

            if (descriptor == null)
            {
                return services.AddSingleton<IOrderedRequestCultureProvider>(new OrderedRequestCultureProvider(provider, order));
            }

            ((IOrderedRequestCultureProvider)descriptor.ImplementationInstance).Order = order;

            return services;
        }

        public static IServiceCollection RemoveOrderedRequestCultureProvider(this IServiceCollection services, Type instanceType)
        {
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IOrderedRequestCultureProvider) &&
                ((IOrderedRequestCultureProvider)s.ImplementationInstance).RequestCultureProvider.GetType() == instanceType);

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            return services;
        }
    }
}