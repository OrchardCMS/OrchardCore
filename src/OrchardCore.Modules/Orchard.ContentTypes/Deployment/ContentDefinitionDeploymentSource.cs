using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.Metadata.Records;
using Orchard.Deployment;
using YesSql;

namespace Orchard.ContentTypes.Deployment
{
    public class ContentDefinitionDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;

        public ContentDefinitionDeploymentSource(ISession session)
        {
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (!(step is ContentDefinitionDeploymentStep contentDefitionStep))
            {
                return;
            }

            var contentTypeDefinitionRecord = await _session
                .Query<ContentDefinitionRecord>()
                .FirstOrDefaultAsync();

            result.Steps.Add(new JObject(
                new JProperty("name", "ContentDefinition"),
                new JProperty("ContentTypes", JArray.FromObject(contentTypeDefinitionRecord.ContentTypeDefinitionRecords)),
                new JProperty("ContentParts", JArray.FromObject(contentTypeDefinitionRecord.ContentPartDefinitionRecords))
            ));

            return;
        }
    }
}
