using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.CustomSettings
{
    public class Permissions : IPermissionProvider
    {
        private static readonly Permission ManageCustomSettings = new Permission("ManageCustomSettings_{0}", "Manage Custom Settings - {0}", new[] { new Permission("ManageResourceSettings") });

        private readonly CustomSettingsService _customSettingsService;

        public Permissions(CustomSettingsService customSettingsService)
        {
            _customSettingsService = customSettingsService;
        }

        public IEnumerable<Permission> GetPermissions()
        {
            var list = new List<Permission>();

            foreach(var type in _customSettingsService.GetSettingsTypes())
            {
                list.Add(CreatePermissionForType(type));
            }

            return list;
        }
     
        public static string CreatePermissionName(string name)
        {
            return String.Format(ManageCustomSettings.Name, name);
        }

        public static Permission CreatePermissionForType(ContentTypeDefinition type)
        {
            return new Permission(
                    CreatePermissionName(type.Name),
                    String.Format(ManageCustomSettings.Description, type.DisplayName),
                    ManageCustomSettings.ImpliedBy
                );
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return Enumerable.Empty<PermissionStereotype>();
        }
    }
}