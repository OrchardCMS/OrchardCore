using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.FileStorage;
using OrchardCore.Security;

namespace OrchardCore.Media.Services
{
    /// <summary>
    /// Check if the path passed as resource is inside the AttachedMediaFieldsFolder
    /// and in case it is, It checks if the user has ManageAttachedMediaFieldsFolder permission
    /// </summary>     
    public class AttachedMediaFieldsFolderAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
        private readonly IMediaFileStore _fileStore;
        private string _pathSeparator;
        private string _mediaFieldsFolder;

        public AttachedMediaFieldsFolderAuthorizationHandler(IServiceProvider serviceProvider,
            AttachedMediaFieldFileService attachedMediaFieldFileService,
            IMediaFileStore fileStore)
        {
            _serviceProvider = serviceProvider;
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
            _fileStore = fileStore;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.HasSucceeded)
            {
                // This handler is not revoking any pre-existing grants.
                return;
            }

            if (requirement.Permission.Name != Permissions.ManageAttachedMediaFieldsFolder.Name)
            {
                return;
            }

            if (context.Resource == null)
            {
                return;
            }

            _pathSeparator = _fileStore.Combine("a", "b").Contains("/") ? "/" : "\\";

            // ensure end trailing slash
            _mediaFieldsFolder = _fileStore.NormalizePath(_attachedMediaFieldFileService.MediaFieldsFolder)
                                .TrimEnd(_pathSeparator.ToCharArray()) + _pathSeparator;

            var path = context.Resource as string;

            if (IsMediaFieldsFolder(path) || IsDescendantOfMediaFieldsFolder(path))
            {
                return;
            }

            // Lazy load to prevent circular dependencies
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

            if (await authorizationService.AuthorizeAsync(context.User, Permissions.ManageOwnMedia))
            {
                context.Succeed(requirement);
            }
        }

        private bool IsMediaFieldsFolder(string childPath)
        {
            // ensure end trailing slash
            childPath = _fileStore.NormalizePath(childPath)
                        .TrimEnd(_pathSeparator.ToCharArray()) + _pathSeparator;

            return childPath.Equals(_mediaFieldsFolder, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsDescendantOfMediaFieldsFolder(string childPath)
        {
            childPath = _fileStore.NormalizePath(childPath);
            return childPath.StartsWith(_mediaFieldsFolder, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
