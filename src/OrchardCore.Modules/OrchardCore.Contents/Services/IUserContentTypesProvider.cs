using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Contents.Services
{
    public interface IUserContentTypesProvider
    {
        Task<IEnumerable<ContentTypeDefinition>> GetCreatableTypesAsync(ClaimsPrincipal user);
        Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync(ClaimsPrincipal user);
    }
}
