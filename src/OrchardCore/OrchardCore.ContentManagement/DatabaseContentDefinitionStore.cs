using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Documents;
using OrchardCore.Data;
using YesSql;

namespace OrchardCore.ContentManagement
{
    public class DatabaseContentDefinitionStore : IContentDefinitionStore
    {
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;

        public DatabaseContentDefinitionStore(ISession session, ISessionHelper sessionHelper)
        {
            _session = session;
            _sessionHelper = sessionHelper;
        }

        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// </summary>
        public Task<ContentDefinitionDocument> LoadContentDefinitionAsync()
            => _sessionHelper.LoadForUpdateAsync<ContentDefinitionDocument>();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// </summary>
        public Task<ContentDefinitionDocument> GetContentDefinitionAsync()
            => _sessionHelper.GetForCachingAsync<ContentDefinitionDocument>();

        public Task SaveContentDefinitionAsync(ContentDefinitionDocument contentDefinitionDocument)
        {
            _session.Save(contentDefinitionDocument);

            return Task.CompletedTask;
        }
    }
}
