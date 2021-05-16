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

            var contentStep = step as ContentDeploymentStep;

            if (contentStep == null)
            {
                return;
            }

            var data = new JArray();

            foreach (var contentItem in await _session.Query<ContentItem, ContentItemIndex>(x => x.Published && x.ContentType.IsIn(contentStep.ContentTypes)).ListAsync())
            {
                var objectData = JObject.FromObject(contentItem);

                // Don't serialize the Id as it could be interpreted as an updated object when added back to YesSql.
                objectData.Remove(nameof(ContentItem.Id));

                if (contentStep.ExportAsSetupRecipe)
                {
                    objectData[nameof(ContentItem.Owner)] = "[js: parameters('AdminUserId')]";
                    objectData[nameof(ContentItem.Author)] = "[js: parameters('AdminUsername')]";
                    objectData[nameof(ContentItem.ContentItemId)] = "[js: uuid()]";
                    objectData.Remove(nameof(ContentItem.ContentItemVersionId));
                    objectData.Remove(nameof(ContentItem.CreatedUtc));
                    objectData.Remove(nameof(ContentItem.ModifiedUtc));
                    objectData.Remove(nameof(ContentItem.PublishedUtc));
                }
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
