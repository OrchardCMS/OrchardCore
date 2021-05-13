using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        private readonly MediaOptions _mediaOptions;
        private char _pathSeparator;
        private string _mediaFieldsFolder;

        public AttachedMediaFieldsFolderAuthorizationHandler(IServiceProvider serviceProvider,
            AttachedMediaFieldFileService attachedMediaFieldFileService,
            IMediaFileStore fileStore,
            IOptions<MediaOptions> options)
        {
            _serviceProvider = serviceProvider;
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
            _fileStore = fileStore;
            _mediaOptions = options.Value;
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

            _pathSeparator = _fileStore.Combine("a", "b").Contains('/') ? '/' : '\\';

            // ensure end trailing slash
            _mediaFieldsFolder = _fileStore.NormalizePath(_attachedMediaFieldFileService.MediaFieldsFolder)
                                .TrimEnd(_pathSeparator) + _pathSeparator;

            var path = context.Resource as string;            

            if (IsMediaFieldsFolder(path) || IsDescendantOfMediaFieldsFolder(path))
            {
                return;
            }

            // Lazy load to prevent circular dependencies
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();
            
            var folderArray = path.Split('/');
            var levelOneFolder = folderArray[0];
            var levelTwoFolder = string.Empty;
            if (folderArray.Count() > 1)
            {
                levelTwoFolder = folderArray[1];
            }

            if (levelOneFolder == _mediaOptions.AssetsUsersFolder)
            {
                if (!await authorizationService.AuthorizeAsync(context.User, Permissions.ManageOthersMedia) && !await authorizationService.AuthorizeAsync(context.User, Permissions.ManageOwnMedia))
                {
                    return;
                }

                if(levelTwoFolder == context.User.Identity.Name)
                {
                    if (!await authorizationService.AuthorizeAsync(context.User, Permissions.ManageOwnMedia))
                    {
                        return;
                    }
                }
                else if(!string.IsNullOrEmpty(levelTwoFolder))
                {
                    if (!await authorizationService.AuthorizeAsync(context.User, Permissions.ManageOthersMedia))
                    {
                        return;
                    }
                }
            }

            //if (levelOneFolder != _mediaOptions.AssetsUsersFolder && path != "")
            //{
            //    if(!await authorizationService.AuthorizeAsync(context.User, Permissions.ManageRootFolderMedia))
            //    {
            //        return;
            //    }                
            //}

            if (await authorizationService.AuthorizeAsync(context.User, Permissions.ManageMedia))
            {
                context.Succeed(requirement);
            }
        }

        private bool IsMediaFieldsFolder(string childPath)
        {
            // ensure end trailing slash
            childPath = _fileStore.NormalizePath(childPath)
                        .TrimEnd(_pathSeparator) + _pathSeparator;

            return childPath.Equals(_mediaFieldsFolder);
        }

        private bool IsDescendantOfMediaFieldsFolder(string childPath)
        {
            childPath = _fileStore.NormalizePath(childPath);
            return childPath.StartsWith(_mediaFieldsFolder, StringComparison.Ordinal);
        }
    }
}
