using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.ContentManagement
{
    public class DatabaseContentDefinitionStore : IContentDefinitionStore
    {
        public DatabaseContentDefinitionStore()
        {
        }

        public async Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            var contentDefinitionRecord = await Session
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
            var session = Session;
            session.Save(contentDefinitionRecord);
            return session.FlushAsync();
        }

        private ISession Session => ShellScope.Services.GetRequiredService<ISession>();
    }
}
