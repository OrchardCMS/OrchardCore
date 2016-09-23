using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Admin
{
    public interface IAdminThemeService : IDependency
    {
        Task<ExtensionDescriptor> GetAdminThemeAsync();
        Task SetAdminThemeAsync(string themeName);
        Task<string> GetAdminThemeNameAsync();
    }
}
