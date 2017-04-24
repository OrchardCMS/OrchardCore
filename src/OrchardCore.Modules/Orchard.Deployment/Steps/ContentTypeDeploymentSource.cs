using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using YesSql;
using YesSql.Services;

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
            // TODO: Batch and create separate content files in the result

            var contentState = step as ContentTypeDeploymentStep;

            if (contentState == null)
            {
                return;
            }

            var data = new JArray();

            foreach (var contentItem in await _session.QueryAsync<ContentItem, ContentItemIndex>(x => x.Published && x.ContentType.IsIn(contentState.ContentTypes)).List())
            {
                data.Add(JObject.FromObject(contentItem));
            }

            if (data.HasValues)
            {
                var jobj = new JObject();
                jobj["name"] = "content";
                jobj["data"] = data;

                result.Steps.Add(jobj);
            }

            return;
        }
    }
}
