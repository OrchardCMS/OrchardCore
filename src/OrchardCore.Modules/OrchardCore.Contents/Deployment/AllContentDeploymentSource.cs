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
            var allContentStep = step as AllContentDeploymentStep;

            if (allContentStep == null)
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

                if (allContentStep.ExportAsSetupRecipe)
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

            return;
        }
    }
}
