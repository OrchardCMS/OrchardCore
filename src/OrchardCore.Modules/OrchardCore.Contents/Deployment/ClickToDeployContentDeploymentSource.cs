using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Deployment
{
    public class ClickToDeployContentDeploymentSource : IDeploymentSource
    {
        private readonly IContentManager _contentManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ClickToDeployContentDeploymentSource(IContentManager contentManager, IUpdateModelAccessor updateModelAccessor)
        {
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var clickToDeployContentDeploymentStep = step as ClickToDeployContentDeploymentStep;

            if (clickToDeployContentDeploymentStep == null)
            {
                return;
            }

            var data = new JArray();
            result.Steps.Add(new JObject(
                new JProperty("name", "Content"),
                new JProperty("data", data)
            ));

            var model = new ClickToDeployModel();
            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(model, "ClickToDeploy", m => m.ContentItemId, m => m.Latest);

            if (!string.IsNullOrEmpty(model.ContentItemId))
            {
                var contentItem = await _contentManager.GetAsync(model.ContentItemId, model.Latest ? VersionOptions.Latest : VersionOptions.Published );
                if (contentItem != null)
                {
                    var objectData = JObject.FromObject(contentItem);
                    objectData.Remove(nameof(ContentItem.Id));
                    data.Add(objectData);
                }
            }

            return;
        }

        public class ClickToDeployModel
        {
            public string ContentItemId { get; set; }
            public bool Latest { get; set; }
        }

    }
}
