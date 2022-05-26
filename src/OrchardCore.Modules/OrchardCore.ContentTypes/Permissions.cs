using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentTypes
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission EditContentTypes = new Permission("EditContentTypes", "Edit all content types.", isSecurityCritical: true);
        public static readonly Permission ViewContentTypes = new Permission("ViewContentTypes", "View content types.", new[] { EditContentTypes });

        private static readonly Permission EditContentType = new Permission("EditContentType_{0}", "Edit content type - {0}", new[] { EditContentTypes });
        private static readonly Permission EditContentPart = new Permission("EditContentPart_{0}", "Edit content part - {0}", new[] { EditContentTypes });

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Permissions(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(GetPermissions());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }

        private IEnumerable<Permission> GetPermissions()
        {
            var permissions = new[]
            {
                ViewContentTypes,
                EditContentTypes
            };

            return permissions
                    .Union(_contentDefinitionManager.ListTypeDefinitions().Select(type => CreatePermissionForType(type)))
                    .Union(_contentDefinitionManager.LoadPartDefinitions().Select(part => CreatePermissionForPart(part)))
                    .ToArray();
        }

        public static Permission CreatePermissionForType(ContentTypeDefinition type)
        {
            return new Permission(
                String.Format(EditContentType.Name, type.Name),
                String.Format(EditContentType.Description, type.DisplayName),
                EditContentType.ImpliedBy
            );
        }

        public static Permission CreatePermissionForPart(ContentPartDefinition part)
        {
            return new Permission(
                String.Format(EditContentPart.Name, part.Name),
                String.Format(EditContentPart.Description, part.Name),
                EditContentPart.ImpliedBy
            );
        }
    }
}
