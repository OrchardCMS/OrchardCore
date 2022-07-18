using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

using OrchardCore.Security.Permissions;


namespace OrchardCore.Contents.Security
{
    /// <summary>
    /// The content type permissions helper generates dynamic permissions per content type.
    /// </summary>
    public class ContentTypePermissionsHelper
    {
        private static readonly Permission PublishContent = new Permission("Publish_{0}", "Publish or unpublish {0} for others", new[] { CommonPermissions.PublishContent });
        private static readonly Permission PublishOwnContent = new Permission("PublishOwn_{0}", "Publish or unpublish {0}", new[] { PublishContent, CommonPermissions.PublishOwnContent });
        private static readonly Permission EditContent = new Permission("Edit_{0}", "Edit {0} for others", new[] { PublishContent, CommonPermissions.EditContent });
        private static readonly Permission EditOwnContent = new Permission("EditOwn_{0}", "Edit {0}", new[] { EditContent, PublishOwnContent, CommonPermissions.EditOwnContent });
        private static readonly Permission DeleteContent = new Permission("Delete_{0}", "Delete {0} for others", new[] { CommonPermissions.DeleteContent });
        private static readonly Permission DeleteOwnContent = new Permission("DeleteOwn_{0}", "Delete {0}", new[] { DeleteContent, CommonPermissions.DeleteOwnContent });
        private static readonly Permission ViewContent = new Permission("View_{0}", "View {0} by others", new[] { EditContent, CommonPermissions.ViewContent });
        private static readonly Permission ViewOwnContent = new Permission("ViewOwn_{0}", "View own {0}", new[] { ViewContent, CommonPermissions.ViewOwnContent });
        private static readonly Permission PreviewContent = new Permission("Preview_{0}", "Preview {0} by others", new[] { EditContent, CommonPermissions.PreviewContent });
        private static readonly Permission PreviewOwnContent = new Permission("PreviewOwn_{0}", "Preview own {0}", new[] { PreviewContent, CommonPermissions.PreviewOwnContent });
        private static readonly Permission CloneContent = new Permission("Clone_{0}", "Clone {0} by others", new[] { EditContent, CommonPermissions.CloneContent });
        private static readonly Permission CloneOwnContent = new Permission("CloneOwn_{0}", "Clone own {0}", new[] { CloneContent, CommonPermissions.CloneOwnContent });
        private static readonly Permission ListContent = new Permission("ListContent_{0}", "List {0} content item(s) owned by all users", new[] { CommonPermissions.ListContent });

        public static readonly Dictionary<string, Permission> PermissionTemplates = new Dictionary<string, Permission>
        {
            { CommonPermissions.PublishContent.Name, PublishContent },
            { CommonPermissions.PublishOwnContent.Name, PublishOwnContent },
            { CommonPermissions.EditContent.Name, EditContent },
            { CommonPermissions.EditOwnContent.Name, EditOwnContent },
            { CommonPermissions.DeleteContent.Name, DeleteContent },
            { CommonPermissions.DeleteOwnContent.Name, DeleteOwnContent },
            { CommonPermissions.ViewContent.Name, ViewContent },
            { CommonPermissions.ViewOwnContent.Name, ViewOwnContent },
            { CommonPermissions.PreviewContent.Name, PreviewContent },
            { CommonPermissions.PreviewOwnContent.Name, PreviewOwnContent },
            { CommonPermissions.CloneContent.Name, CloneContent },
            { CommonPermissions.CloneOwnContent.Name, CloneOwnContent },
            { CommonPermissions.ListContent.Name, ListContent }
        };

        public static Dictionary<ValueTuple<string, string>, Permission> PermissionsByType = new Dictionary<ValueTuple<string, string>, Permission>();

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
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            return new Permission(
                String.Format(template.Name, typeDefinition.Name),
                String.Format(template.Description, typeDefinition.DisplayName),
                (template.ImpliedBy ?? Array.Empty<Permission>()).Where(t => t != null).Select(t => CreateDynamicPermission(t, typeDefinition))
            )
            {
                Category = $"{typeDefinition.DisplayName} Content Type - {typeDefinition.Name}",
            };
        }

        /// <summary>
        /// Generates a permission dynamically for a content type, without a display name or category
        /// </summary>
        public static Permission CreateDynamicPermission(Permission template, string contentType)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var key = new ValueTuple<string, string>(template.Name, contentType);

            if (PermissionsByType.TryGetValue(key, out var permission))
            {
                return permission;
            }

            permission = new Permission(
                String.Format(template.Name, contentType),
                String.Format(template.Description, contentType),
                (template.ImpliedBy ?? new Permission[0]).Select(t => CreateDynamicPermission(t, contentType))
            );

            var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(PermissionsByType);
            localPermissions[key] = permission;
            PermissionsByType = localPermissions;

            return permission;
        }
    }
}
