using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationPartContentAdminFilter : IContentAdminFilter
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public LocalizationPartContentAdminFilter(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task ApplyRouteValues(ListContentsViewModel model, IUpdateModel updateModel, RouteValueDictionary routeValueDictionary)
        {
            var viewModel = new LocalizationContentAdminFilterModel();
            if (await updateModel.TryUpdateModelAsync(viewModel, "Localization"))
            {
                if (viewModel.ShowLocalizedContentTypes)
                {
                    routeValueDictionary.TryAdd("Localization.ShowLocalizedContentTypes", viewModel.ShowLocalizedContentTypes);
                }

                if (!string.IsNullOrEmpty(viewModel.SelectedCulture))
                {
                    routeValueDictionary.TryAdd("Localization.SelectedCulture", viewModel.SelectedCulture);
                }
            }
        }

        public async Task FilterAsync(IQuery<ContentItem> query, ListContentsViewModel model, PagerParameters pagerParameters, IUpdateModel updateModel)
        {
            var viewModel = new LocalizationContentAdminFilterModel();
            if (await updateModel.TryUpdateModelAsync(viewModel, "Localization"))
            {
                // Show list content items
                if (viewModel.ShowLocalizedContentTypes)
                {
                    var localizedTypes = _contentDefinitionManager
                        .ListTypeDefinitions()
                        .Where(x =>
                            x.Parts.Any(p =>
                                p.PartDefinition.Name == nameof(LocalizationPart)))
                        .Select(x => x.Name);

                    query.With<ContentItemIndex>(x => x.ContentType.IsIn(localizedTypes));
                    // This no longer works because of query weirdness. but I don't think we really need it. (it's weird anyway to do this)
                    model.Options.SelectedContentType = null;
                }

                // Show contained elements for the specified list
                else if (!String.IsNullOrEmpty(viewModel.SelectedCulture))
                {
                    query.With<LocalizedContentItemIndex>(x => x.Culture == viewModel.SelectedCulture);
                }
            }
        }
    }

    public class LocalizationContentAdminFilterModel
    {
        public bool ShowLocalizedContentTypes { get; set; }
        public string SelectedCulture { get; set; }

        [BindNever]
        public List<SelectListItem> Cultures { get; set; }
    }
}
