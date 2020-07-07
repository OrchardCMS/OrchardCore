using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.ViewModels;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Lists.Services
{
    public class ListPartContentsAdminListFilter : IContentsAdminListFilter
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ListPartContentsAdminListFilter(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var viewModel = new ListPartContentsAdminFilterViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, nameof(ListPart)))
            {
                // Show list content items
                if (viewModel.ShowListContentTypes)
                {
                    var listableTypes = _contentDefinitionManager
                        .ListTypeDefinitions()
                        .Where(x =>
                            x.Parts.Any(p =>
                                p.PartDefinition.Name == nameof(ListPart)))
                        .Select(x => x.Name);

                    query.With<ContentItemIndex>(x => x.ContentType.IsIn(listableTypes));
                }

                // Show contained elements for the specified list
                else if (viewModel.ListContentItemId != null)
                {
                    query.With<ContainedPartIndex>(x => x.ListContentItemId == viewModel.ListContentItemId);
                }
            }
        }
    }
}
