using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentFields.Services;
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

        public async Task<IActionResult> List(string[] contentTypes, string searchTerm, PagerParameters pagerParameters)
        {
            // TODO: get content types from the field settings instead of having to pass them in?
            if (!contentTypes.Any())
            {
                return BadRequest("At least one content type is required");
            }

            var searchResults = new List<ContentPickerResult>();
            foreach (var resultProvider in _resultProviders)
            {
                searchResults.AddRange(await resultProvider.GetContentItems(searchTerm, contentTypes));
            }

            // TODO: handle pagination

            return new ObjectResult(searchResults);
        }
    }

    public class ContentPickerResult
    {
        public string DisplayText { get; set; }
        public string ContentItemId { get; set; }
        public decimal Score { get; set; }
    }
}
