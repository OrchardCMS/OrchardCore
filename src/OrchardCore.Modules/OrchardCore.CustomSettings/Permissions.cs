using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.CustomSettings
{
    public class Permissions : IPermissionProvider
    {
        private static readonly Permission _manageCustomSettings = new("ManageCustomSettings_{0}", "Manage Custom Settings - {0}", new[] { new Permission("ManageResourceSettings") });

        private readonly CustomSettingsService _customSettingsService;

        public Permissions(CustomSettingsService customSettingsService)
        {
            _customSettingsService = customSettingsService;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var list = new List<Permission>();

            foreach (var type in _customSettingsService.GetAllSettingsTypes())
            {
                list.Add(CreatePermissionForType(type));
            }

            return Task.FromResult(list.AsEnumerable());
        }

        public static string CreatePermissionName(string name)
        {
            return String.Format(_manageCustomSettings.Name, name);
        }

        public static Permission CreatePermissionForType(ContentTypeDefinition type)
        {
            return new Permission(
                    CreatePermissionName(type.Name),
                    String.Format(_manageCustomSettings.Description, type.DisplayName),
                    _manageCustomSettings.ImpliedBy
                );
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}
