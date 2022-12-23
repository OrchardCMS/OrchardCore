using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Resources.Liquid;

namespace OrchardCore.Resources
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<LiquidViewOptions>(o =>
            {
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserTag("meta", parser.ArgumentsListParser, MetaTag.WriteToAsync));
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserTag("link", parser.ArgumentsListParser, LinkTag.WriteToAsync));
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserTag("script", parser.ArgumentsListParser, ScriptTag.WriteToAsync));
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserTag("style", parser.ArgumentsListParser, StyleTag.WriteToAsync));
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserTag("resources", parser.ArgumentsListParser, ResourcesTag.WriteToAsync));
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserBlock("scriptblock", parser.ArgumentsListParser, ScriptBlock.WriteToAsync));
                o.LiquidViewParserConfiguration.Add(parser => parser.RegisterParserBlock("styleblock", parser.ArgumentsListParser, StyleBlock.WriteToAsync));
            });

            serviceCollection.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();

            var resourceConfiguration = _shellConfiguration.GetSection("OrchardCore_Resources");
            serviceCollection.Configure<ResourceOptions>(resourceConfiguration);
        }
    }
}
