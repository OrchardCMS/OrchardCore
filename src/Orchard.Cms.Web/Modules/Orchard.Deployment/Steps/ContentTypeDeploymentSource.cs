using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Recipes.Models;
using YesSql.Core.Services;

namespace Orchard.Deployment.Steps
{
    public class ContentTypeDeploymentSource : IDeploymentSource
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContentTypeDeploymentSource(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var contentState = step as ContentTypeDeploymentStep;

            if (contentState == null)
            {
                return;
            }

            var data = new JArray();
            var descriptor = new RecipeStepDescriptor { Name = "Data", Step = data };

            foreach (var contentItem in await _session.QueryAsync<ContentItem, ContentItemIndex>(x => x.Published && x.ContentType == contentState.ContentType).List())
            {
                data.Add(contentItem.Content);
            }

            return;
        }
    }
}
