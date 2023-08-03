using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.AmazonS3
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ViewAmazonS3MediaOptions = new(nameof(ViewAmazonS3MediaOptions), "View Amazon S3 Media Options");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ViewAmazonS3MediaOptions }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ViewAmazonS3MediaOptions },
                },
            };
        }
    }
}
