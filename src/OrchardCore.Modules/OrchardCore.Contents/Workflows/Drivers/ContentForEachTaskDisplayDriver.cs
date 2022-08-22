using OrchardCore.Workflows.Display;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{


    public class ContentForEachTaskDisplayDriver: ActivityDisplayDriver<ContentForEachTask, ContentForEachTaskViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public ContentForEachTaskDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }
        protected override void EditActivity(ContentForEachTask activity, ContentForEachTaskViewModel model)
        {
            model.AvailableContentTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name })
                .ToList();
            model.ContentType = activity.ContentType;
            model.Take = activity.Take;
            model.Published = activity.Published;
        }

        protected override void UpdateActivity(ContentForEachTaskViewModel model, ContentForEachTask activity)
        {
            activity.ContentType = model.ContentType;
            activity.Published = model.Published;
            activity.Take = model.Take;
        }
    }

}
