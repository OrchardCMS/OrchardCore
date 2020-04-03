using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;
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

        public async Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            var contentDefinitionRecord = await _session
                .Query<ContentDefinitionRecord>()
                .FirstOrDefaultAsync();

            if (contentDefinitionRecord == null)
            {
                contentDefinitionRecord = new ContentDefinitionRecord();
                await SaveContentDefinitionAsync(contentDefinitionRecord);
            }

            return contentDefinitionRecord;
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        {
            _session.Save(contentDefinitionRecord);

            return Task.CompletedTask;
        }
    }
}
