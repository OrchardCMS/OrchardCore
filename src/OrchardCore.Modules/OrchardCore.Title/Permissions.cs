using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Title
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission EditTitlePart = new Permission(nameof(EditTitlePart), "Edit any titles.");
        internal static readonly Permission EditTitlePartTemplate = new Permission("EditTitle_{0}", "Edit the title of {0}", new[] { EditTitlePart });

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
            new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { EditTitlePart },
                },
            };
        
        public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
            Task.FromResult(new[] { EditTitlePart }.AsEnumerable());        
    }
}
