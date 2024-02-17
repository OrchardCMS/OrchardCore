using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Deployment
{
    public class AllSitemapsDeploymentSource : IDeploymentSource
    {
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

            var jArray = JArray.FromObject(sitemaps);

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Sitemaps",
                ["data"] = jArray,
            });
        }
    }
}
