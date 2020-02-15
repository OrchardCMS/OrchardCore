using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace TheMathesisTheme
{
	public class Startup : StartupBase
	{
		public override void ConfigureServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddScoped<IResourceManifestProvider, ResourceManifest>();
		}
	}
}