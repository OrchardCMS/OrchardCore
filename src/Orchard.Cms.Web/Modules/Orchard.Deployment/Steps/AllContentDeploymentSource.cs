using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Recipes.Models;
using YesSql.Core.Services;

namespace Orchard.Deployment.Steps
{
    public class AllContentDeploymentSource : IDeploymentSource
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public AllContentDeploymentSource(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
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
            var descriptor = new RecipeStepDescriptor { Name = "Data", Step = data };

            foreach (var contentItem in await _session.QueryAsync<ContentItem, ContentItemIndex>(x => x.Published).List())
            {
                data.Add(contentItem.Content);
            }

            return;
        }
    }
}
