using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Testing.Context;

namespace OrchardCore.Tests.Apis.Context
{
    public class AgencyContext : SiteContext
    {
        public AgencyContext()
        {
            this.WithRecipe("Agency");
        }
    }
}
