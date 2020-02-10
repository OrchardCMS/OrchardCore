using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.Security
{
    public class ContentTypePermissions : IPermissionProvider
    {
        private static readonly Permission PublishContent = new Permission("Publish_{0}", "Publish or unpublish {0} for others", new[] { Permissions.PublishContent });
        private static readonly Permission PublishOwnContent = new Permission("PublishOwn_{0}", "Publish or unpublish {0}", new[] { PublishContent, Permissions.PublishOwnContent });
        private static readonly Permission EditContent = new Permission("Edit_{0}", "Edit {0} for others", new[] { PublishContent, Permissions.EditContent });
        private static readonly Permission EditOwnContent = new Permission("EditOwn_{0}", "Edit {0}", new[] { EditContent, PublishOwnContent, Permissions.EditOwnContent });
        private static readonly Permission DeleteContent = new Permission("Delete_{0}", "Delete {0} for others", new[] { Permissions.DeleteContent });
        private static readonly Permission DeleteOwnContent = new Permission("DeleteOwn_{0}", "Delete {0}", new[] { DeleteContent, Permissions.DeleteOwnContent });
        private static readonly Permission ViewContent = new Permission("View_{0}", "View {0} by others", new[] { EditContent, Permissions.ViewContent });
        private static readonly Permission ViewOwnContent = new Permission("ViewOwn_{0}", "View own {0}", new[] { ViewContent, Permissions.ViewOwnContent });
        private static readonly Permission PreviewContent = new Permission("Preview_{0}", "Preview {0} by others", new[] { EditContent, Permissions.PreviewContent });
        private static readonly Permission PreviewOwnContent = new Permission("PreviewOwn_{0}", "Preview own {0}", new[] { PreviewContent, Permissions.PreviewOwnContent });

        public static readonly Dictionary<string, Permission> PermissionTemplates = new Dictionary<string, Permission> {
            {Permissions.PublishContent.Name, PublishContent},
            {Permissions.PublishOwnContent.Name, PublishOwnContent},
            {Permissions.EditContent.Name, EditContent},
            {Permissions.EditOwnContent.Name, EditOwnContent},
            {Permissions.DeleteContent.Name, DeleteContent},
            {Permissions.DeleteOwnContent.Name, DeleteOwnContent},
            {Permissions.ViewContent.Name, ViewContent},
            {Permissions.ViewOwnContent.Name, ViewOwnContent},
            {Permissions.PreviewContent.Name, PreviewContent},
            {Permissions.PreviewOwnContent.Name, PreviewOwnContent}
        };

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypePermissions(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            // manage rights only for Securable types
            var securableTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.GetSettings<ContentTypeSettings>().Securable);

            var result = new List<Permission>();

            foreach (var typeDefinition in securableTypes)
            {
                foreach (var permissionTemplate in PermissionTemplates.Values)
                {
                    result.Add(CreateDynamicPermission(permissionTemplate, typeDefinition));
                }
            }

            return Task.FromResult(result.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return Enumerable.Empty<PermissionStereotype>();
        }

        /// <summary>
        /// Returns a dynamic permission for a content type, based on a global content permission template
        /// </summary>
        public static Permission ConvertToDynamicPermission(Permission permission)
        {
            if (PermissionTemplates.TryGetValue(permission.Name, out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Generates a permission dynamically for a content type
        /// </summary>
        public static Permission CreateDynamicPermission(Permission template, ContentTypeDefinition typeDefinition)
        {
            return new Permission(
                String.Format(template.Name, typeDefinition.Name),
                String.Format(template.Description, typeDefinition.DisplayName),
                (template.ImpliedBy ?? new Permission[0]).Select(t => CreateDynamicPermission(t, typeDefinition))
            )
            {
                Category = typeDefinition.DisplayName
            };
        }

        /// <summary>
        /// Generates a permission dynamically for a content type, without a display name or category
        /// </summary>
        public static Permission CreateDynamicPermission(Permission template, string contentType)
        {
            return new Permission(
                String.Format(template.Name, contentType),
                String.Format(template.Description, contentType),
                (template.ImpliedBy ?? new Permission[0]).Select(t => CreateDynamicPermission(t, contentType))
            );
        }
    }
}
