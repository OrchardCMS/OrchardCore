using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Content.Controllers
{
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

            var fieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields
                .FirstOrDefault(f => f.Name == field);

            var fieldSettings = fieldDefinition?.Settings.ToObject<ContentPickerFieldSettings>();
            if (fieldSettings == null)
            {
                return BadRequest("Unable to find field definition");
            }

            var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == fieldSettings.SearchResultProvider);
            if (resultProvider == null)
            {
                return new ObjectResult(new List<ContentPickerResult>());
            }

            var results = await resultProvider.Search(new ContentPickerSearchContext
            {
                Query = query,
                ContentTypes = fieldSettings.DisplayedContentTypes,
                IndexName = fieldSettings.SearchIndex
            });

            return new ObjectResult(results);
        }
    }
}
