using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.Handlers
{
    public class TaxonomyContentHandler : ContentHandlerBase
    {
        public override Task ActivatedAsync(ActivatedContentContext context)
        {
            // Adding mandatory parts to taxonomy content items such that they can't be removed from the admin UI
            //if (context.ContentItem.ContentType == "Taxonomy")
            //{
            //    context.ContentItem.Weld<TaxonomyPart>(new { Position = "3" });
            //    context.ContentItem.Weld<TermsListPart>(new { Position = "4" });
            //}

            return Task.CompletedTask;
        }
    }
}