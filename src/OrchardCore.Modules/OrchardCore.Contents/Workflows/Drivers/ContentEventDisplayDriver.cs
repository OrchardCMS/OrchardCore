using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public abstract class ContentEventDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity, TViewModel> where TActivity : ContentEvent where TViewModel : ContentEventViewModel<TActivity>, new()
    {
        protected ContentEventDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            ContentDefinitionManager = contentDefinitionManager;
        }

        protected IContentDefinitionManager ContentDefinitionManager { get; }

        protected override void EditActivity(TActivity source, TViewModel target)
        {
            target.SelectedContentTypeNames = source.ContentTypeFilter;
        }

        public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
        {
            var viewModel = new TViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, x => x.SelectedContentTypeNames))
            {
                model.ContentTypeFilter = FilterContentTypesQuery(viewModel.SelectedContentTypeNames).ToList();
            }
            return Edit(model);
        }

        public override IDisplayResult Display(TActivity activity)
        {
            return Combine(
                Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ContentEventViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
                Factory($"{typeof(TActivity).Name}_Fields_Design", ctx =>
                {
                    var contentTypeDefinitions = ContentDefinitionManager.ListTypeDefinitions().ToDictionary(x => x.Name);
                    var selectedContentTypeDefinitions = activity.ContentTypeFilter.Select(x => contentTypeDefinitions[x]).ToList();

                    var shape = new ContentEventViewModel<TActivity>
                    {
                        ContentTypeFilter = selectedContentTypeDefinitions,
                        Activity = activity,
                    };

                    return shape;
                }).Location("Design", "Content")
            );
        }

        /// <summary>
        /// Filters out any content type that doesn't exist.
        /// </summary>
        protected IEnumerable<string> FilterContentTypesQuery(IEnumerable<string> contentTypeNames)
        {
            var contentTypeDefinitions = ContentDefinitionManager.ListTypeDefinitions().ToDictionary(x => x.Name);
            return contentTypeNames.Where(x => !String.IsNullOrWhiteSpace(x) && contentTypeDefinitions.ContainsKey(x));
        }
    }
}
