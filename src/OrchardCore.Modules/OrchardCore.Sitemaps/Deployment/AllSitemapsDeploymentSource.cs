using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Deployment
{
    public class AllSitemapsDeploymentSource : IDeploymentSource
    {
        private readonly ISitemapManager _sitemapManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllSitemapsDeploymentSource(
            ISitemapManager sitemapManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _sitemapManager = sitemapManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not AllSitemapsDeploymentStep)
            {
                return;
            }

            var sitemaps = await _sitemapManager.GetSitemapsAsync();

            var jArray = JArray.FromObject(sitemaps, _jsonSerializerOptions);

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Sitemaps",
                ["data"] = jArray,
            });
        }
    }
}
