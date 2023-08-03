using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Google
{
    public class Permissions
    {
        public static readonly Permission ManageGoogleAuthentication
            = new("ManageGoogleAuthentication", "Manage Google Authentication settings");

        public static readonly Permission ManageGoogleAnalytics
            = new("ManageGoogleAnalytics", "Manage Google Analytics settings");

        public static readonly Permission ManageGoogleTagManager
            = new("ManageGoogleTagManager", "Manage Google Tag Manager settings");

        public class GoogleAuthentication : IPermissionProvider
        {
            public Task<IEnumerable<Permission>> GetPermissionsAsync()
            {
                return Task.FromResult(new[]
                {
                    ManageGoogleAuthentication,
                }
                .AsEnumerable());
            }

            public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            {
                yield return new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        ManageGoogleAuthentication,
                    }
                };
            }
        }

        public class GoogleAnalytics : IPermissionProvider
        {
            public Task<IEnumerable<Permission>> GetPermissionsAsync()
            {
                return Task.FromResult(new[]
                {
                    ManageGoogleAnalytics,
                }
                .AsEnumerable());
            }

            public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            {
                yield return new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        ManageGoogleAnalytics,
                    }
                };
            }
        }

        public class GoogleTagManager : IPermissionProvider
        {
            public Task<IEnumerable<Permission>> GetPermissionsAsync()
            {
                return Task.FromResult(new[]
                {
                    ManageGoogleTagManager,
                }
                .AsEnumerable());
            }

            public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
            {
                yield return new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        ManageGoogleTagManager,
                    }
                };
            }
        }
    }
}
