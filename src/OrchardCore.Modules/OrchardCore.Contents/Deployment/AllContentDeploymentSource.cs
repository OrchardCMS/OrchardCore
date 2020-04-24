using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using YesSql;

namespace OrchardCore.Contents.Deployment
{
    public class AllContentDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;

        public AllContentDeploymentSource(ISession session)
        {
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allContentState = step as AllContentDeploymentStep;

            if (allContentState == null)
            {
                return;
            }

            var data = new JArray();
            result.Steps.Add(new JObject(
                new JProperty("name", "Content"),
                new JProperty("data", data)
            ));

            foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published).ListAsync())
            {
                var objectData = JObject.FromObject(contentItem);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql
                objectData.Remove(nameof(ContentItem.Id));
                data.Add(objectData);
            }

            return;
        }
    }
}
