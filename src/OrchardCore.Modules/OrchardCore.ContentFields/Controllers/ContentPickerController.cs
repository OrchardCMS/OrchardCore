using System.Collections.Generic;
using System.Linq;
using OrchardCore.Admin;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Content.Controllers
{
    [Admin]
    public class ContentPickerController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPickerResultProvider> _resultProviders;

        public ContentPickerController(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPickerResultProvider> resultProviders
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
        }

        public async Task<IActionResult> List(string part, string field, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
            {
                return BadRequest("Part and field are required parameters");
            }

            var partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields
                .FirstOrDefault(f => f.Name == field);

            var fieldSettings = partFieldDefinition?.Settings.ToObject<ContentPickerFieldSettings>();
            if (fieldSettings == null)
            {
                return BadRequest("Unable to find field definition");
            }

            var editor = partFieldDefinition.Editor() ?? "Default";
            var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor);
            if (resultProvider == null)
            {
                return new ObjectResult(new List<ContentPickerResult>());
            }

            var results = await resultProvider.Search(new ContentPickerSearchContext
            {
                Query = query,
                ContentTypes = fieldSettings.DisplayedContentTypes,
                PartFieldDefinition = partFieldDefinition
            });

            return new ObjectResult(results);
        }
    }
}
