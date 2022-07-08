using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Apis.Context
{
    public class AgencyContext : SiteContext
    {
        public static IShellHost ShellHost { get; }

        static AgencyContext()
        {
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        public AgencyContext()
        {
            this.WithRecipe("Agency");
        }
    }
}
