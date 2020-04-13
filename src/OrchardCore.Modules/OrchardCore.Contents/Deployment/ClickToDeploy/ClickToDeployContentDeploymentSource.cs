using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Deployment.ClickToDeploy
{
    public class ClickToDeployContentDeploymentSource : IDeploymentSource
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ClickToDeployContentDeploymentSource(
            IContentManager contentManager,
            ISession session,
            IUpdateModelAccessor updateModelAccessor)
        {
            _contentManager = contentManager;
            _session = session;
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
            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(model, "ClickToDeploy", m => m.ItemIds, m => m.Latest, m => m.ContentItemId);

            if (!string.IsNullOrEmpty(model.ContentItemId))
            {
                var contentItem = await _contentManager.GetAsync(model.ContentItemId, model.Latest ? VersionOptions.Latest : VersionOptions.Published);
                if (contentItem != null)
                {
                    var objectData = JObject.FromObject(contentItem);
                    objectData.Remove(nameof(ContentItem.Id));
                    data.Add(objectData);
                }
            }

            if (model.ItemIds?.Count() > 0)
            {
                var checkedContentItems = await _session.Query<ContentItem, ContentItemIndex>().Where(x => x.DocumentId.IsIn(model.ItemIds) && x.Published).ListAsync();

                foreach (var contentItem in checkedContentItems)
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
            public IEnumerable<int> ItemIds { get; set; }
            public string ContentItemId { get; set; }
            public bool Latest { get; set; }
        }

    }
}
