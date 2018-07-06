using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization
{
    public interface IOrderedRequestCultureProvider
    {
        int Order { get; }
        IRequestCultureProvider RequestCultureProvider { get; }
    }

    public class OrderedRequestCultureProvider : IOrderedRequestCultureProvider
    {
        public OrderedRequestCultureProvider(IRequestCultureProvider requestCultureProvider, int order = 0)
        {
            Order = order;
            RequestCultureProvider = requestCultureProvider;
        }

        public int Order { get; }
        public IRequestCultureProvider RequestCultureProvider { get; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderedRequestCultureProvider(this IServiceCollection services, IRequestCultureProvider requestCultureProvider, int order = 0)
        {
            return services.AddSingleton<IOrderedRequestCultureProvider>(sp =>
            {
                if (requestCultureProvider is RequestCultureProvider provider)
                {
                    provider.Options = sp.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
                }

                return new OrderedRequestCultureProvider(requestCultureProvider, order);
            });
        }
    }
}