using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public class TenantApplicationBuilder
    {
        public TenantApplicationBuilder(IApplicationBuilder builder)
        {
            ApplicationBuilder = builder;
        }

        public IApplicationBuilder ApplicationBuilder { get; }
    }
}
