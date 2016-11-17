using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Models;
using Orchard.Indexing;

namespace Orchard.Identity.Indexing
{
    public class AspectsContentIndexHandler : IContentItemIndexHandler
    {
        private readonly IContentManager _contentManager;

        public AspectsContentIndexHandler(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Task BuildIndexAsync(BuildIndexContext context)
        {
            var body = _contentManager.GetAspect<IBodyAspect>(context.ContentItem);

            if (body != null)
            {
                context.DocumentIndex.Entries.Add(
                "Content.IBodyAspect.Body",
                new DocumentIndex.DocumentIndexEntry(
                    body.Body,
                    DocumentIndex.Types.Text,
                    DocumentIndexOptions.Analyze | DocumentIndexOptions.Sanitize));
            }

            return Task.CompletedTask;
        }
    }
}
