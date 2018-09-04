using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Navigation;

namespace OrchardCore.Content.Controllers
{
    public class ContentPickerController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPickerResultProvider> _resultProviders;

        public ContentPickerController(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentPickerResultProvider> resultProviders)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
        }

        public async Task<IActionResult> List(string part, string field, string query, PagerParameters pagerParameters)
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

            var searchResults = new List<ContentPickerResult>();
            foreach (var resultProvider in _resultProviders)
            {
                searchResults.AddRange(await resultProvider.Search(new ContentPickerSearchContext
                {
                    Query = query,
                    ContentTypes = fieldSettings.DisplayedContentTypes
                }));
            }

            return new ObjectResult(searchResults);
        }
    }
}
