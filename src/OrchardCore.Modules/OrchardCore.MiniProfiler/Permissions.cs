using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.MiniProfiler
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ViewMiniProfilerOnFrontEnd = new("ViewMiniProfilerOnFrontEnd", "View Mini Profiler widget on front end pages");
        public static readonly Permission ViewMiniProfilerOnBackEnd = new("ViewMiniProfilerOnBackEnd", "View Mini Profiler widget on back end pages");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ViewMiniProfilerOnFrontEnd, ViewMiniProfilerOnBackEnd }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ViewMiniProfilerOnFrontEnd, ViewMiniProfilerOnBackEnd },
                }
            };
        }
    }
}
