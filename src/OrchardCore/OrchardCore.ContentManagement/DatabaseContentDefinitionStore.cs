using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Documents;
using YesSql;

namespace OrchardCore.ContentManagement
{
    public class DatabaseContentDefinitionStore : IContentDefinitionStore
    {
        private readonly ISession _session;

        public DatabaseContentDefinitionStore(ISession session)
        {
            _session = session;
        }

        public async Task<ContentDefinitionDocument> LoadContentDefinitionAsync()
        {
            var contentDefinitionRecord = await _session
                .Query<ContentDefinitionDocument>()
                .FirstOrDefaultAsync();

            if (contentDefinitionRecord == null)
            {
                contentDefinitionRecord = new ContentDefinitionDocument();
                await SaveContentDefinitionAsync(contentDefinitionRecord);
            }

            return contentDefinitionRecord;
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionDocument contentDefinitionRecord)
        {
            _session.Save(contentDefinitionRecord);

            return Task.CompletedTask;
        }
    }
}
