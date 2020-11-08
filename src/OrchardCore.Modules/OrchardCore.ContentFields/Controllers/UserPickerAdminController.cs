using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.Services;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Controllers
{
    [RequireFeatures("OrchardCore.Users")]
    [Admin]
    public class UserPickerAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IUserPickerResultProvider> _resultProviders;

        public UserPickerAdminController(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IUserPickerResultProvider> resultProviders
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _resultProviders = resultProviders;
        }

        public async Task<IActionResult> SearchUsers(string part, string field, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
            {
                return BadRequest("Part and field are required parameters");
            }

            var partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields
                .FirstOrDefault(f => f.Name == field);

            var fieldSettings = partFieldDefinition?.GetSettings<UserPickerFieldSettings>();
            if (fieldSettings == null)
            {
                return BadRequest("Unable to find field definition");
            }

            var editor = partFieldDefinition.Editor() ?? "Default";
            var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor);
            if (resultProvider == null)
            {
                return new ObjectResult(new List<UserPickerResult>());
            }

            var results = await resultProvider.Search(new UserPickerSearchContext
            {
                Query = query,
                DisplayAllUsers = fieldSettings.DisplayAllUsers,
                Roles = fieldSettings.DisplayedRoles,
                PartFieldDefinition = partFieldDefinition
            });

            return new ObjectResult(results.Select(r => new VueMultiselectUserViewModel() { Id = r.UserId, DisplayText = r.DisplayText, IsEnabled = r.IsEnabled }));
        }
    }
}
