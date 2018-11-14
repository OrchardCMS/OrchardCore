using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Liquid.Services
{
    public class ConfigurationLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IConfiguration _configuration;

        public ConfigurationLiquidTemplateEventHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            context.LocalScope.SetValue("Configuration", new Configuration());
            context.MemberAccessStrategy.Register<Configuration, FluidValue>((configuration, name) => FluidValue.Create(_configuration.GetValue<string>(name)));
            return Task.CompletedTask;
        }

        private class Configuration { }
    }
}