using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Deployment;
using YesSql;

namespace OrchardCore.ContentTypes.Deployment
{
    public class AllContentDefinitionDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;

        public AllContentDefinitionDeploymentSource(ISession session)
        {
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (!(step is AllContentDefinitionDeploymentStep contentDefitionStep))
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
