using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Deployment
{
    public class ContentDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;

        public ContentDeploymentSource(ISession session)
        {
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            // TODO: Batch and create separate content files in the result.

            var contentState = step as ContentDeploymentStep;

            if (contentState == null)
            {
                return;
            }

            var data = new JArray();

            foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.ContentType.IsIn(contentState.ContentTypes)).ListAsync())
            {
                var objectData = JObject.FromObject(contentItem);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql.
                objectData.Remove(nameof(ContentItem.Id));
                data.Add(objectData);
            }

            if (data.HasValues)
            {
                var jobj = new JObject
                {
                    ["name"] = "content",
                    ["data"] = data
                };

                result.Steps.Add(jobj);
            }
        }
    }
}
