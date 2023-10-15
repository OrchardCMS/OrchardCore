using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Deployment;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Deployment
{
    public class AllSitemapsDeploymentSource : IDeploymentSource
    {
        private static readonly JsonSerializer _serializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        private readonly ISitemapManager _sitemapManager;

        public AllSitemapsDeploymentSource(ISitemapManager sitemapManager)
        {
            _sitemapManager = sitemapManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllSitemapsDeploymentStep)
            {
                return;
            }

            var sitemaps = await _sitemapManager.GetSitemapsAsync();

            var jArray = System.Text.Json.JsonSerializer(sitemaps, _serializer);

            result.AddSimpleStep("Sitemaps", "data", jArray);
        }
    }
}
