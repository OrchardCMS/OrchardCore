using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Controllers
{
    [RequireFeatures("OrchardCore.Users")]
    [Admin]
    public class UserPickerAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IUserPickerResultProvider> _resultProviders;

        public UserPickerAdminController(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IEnumerable<IUserPickerResultProvider> resultProviders
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _resultProviders = resultProviders;
        }

        public async Task<IActionResult> SearchUsers(string part, string field, string contentType, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || String.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(contentType))
            {
                return BadRequest("Part, field and contentType are required parameters");
            }

            var contentItem = await _contentManager.NewAsync(contentType);
            contentItem.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields
                .FirstOrDefault(f => f.Name == field);

            var fieldSettings = partFieldDefinition?.GetSettings<UserPickerFieldSettings>();
            if (fieldSettings == null)
            {
                return BadRequest("Unable to find field definition");
            }

            var editor = partFieldDefinition.Editor() ?? "Default";

            var resultProvider = _resultProviders.FirstOrDefault(p => p.Name == editor)
                ?? _resultProviders.FirstOrDefault(p => p.Name == "Default");

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
