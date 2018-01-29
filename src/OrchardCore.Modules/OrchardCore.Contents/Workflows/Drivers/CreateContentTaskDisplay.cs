using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.Contents.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Drivers
{
    public class CreateContentTaskDisplay : ActivityDisplayDriver<CreateContentTask, CreateContentTaskViewModel>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public CreateContentTaskDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override void Map(CreateContentTask source, CreateContentTaskViewModel target)
        {
            target.AvailableContentTypes = _contentDefinitionManager.ListTypeDefinitions().Where(x => x.Settings.ToObject<ContentTypeSettings>().Creatable).Select(x => new SelectListItem { Text = x.DisplayName, Value = x.Name }).ToList();
            target.ContentType = source.ContentType;
            target.Publish = source.Publish;
            target.ContentProperties = source.ContentProperties.Expression;
        }

        protected override void Map(CreateContentTaskViewModel source, CreateContentTask target)
        {
            target.ContentType = source.ContentType;
            target.Publish = source.Publish;
            target.ContentProperties = new WorkflowExpression<string>(source.ContentProperties);
        }
    }
}
