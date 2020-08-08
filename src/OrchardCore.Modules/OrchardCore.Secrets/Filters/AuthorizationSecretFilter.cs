using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Secrets.Filters
{
    public class AuthorizationSecretFilter : ISecretLiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var servicesValue))
            {
                throw new ArgumentException("Services missing while invoking 'auth_secret'");
            }

            var key = input.ToStringValue();
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key is empty while invoking 'auth_secret'");
            }

            var secretSecret = ((IServiceProvider)servicesValue).GetRequiredService<ISecretService<AuthorizationSecret>>();

            var authorizationSecret = await secretSecret.GetSecretAsync(key);
            if (authorizationSecret == null)
            {
                return NilValue.Instance;
            }

            return new StringValue(authorizationSecret.AuthenticationString);
        }
    }
}
