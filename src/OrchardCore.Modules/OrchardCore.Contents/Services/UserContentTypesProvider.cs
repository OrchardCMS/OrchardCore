using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Contents.Services
{
    public class UserContentTypesProvider : IUserContentTypesProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        public UserContentTypesProvider(
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IContentManager contentManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _authorizationService = authorizationService;
            _contentManager = contentManager;
        }

        public async Task<IEnumerable<ContentTypeDefinition>> GetCreatableTypesAsync(ClaimsPrincipal user)
        {
            var creatable = new List<ContentTypeDefinition>();

            if(user == null)
            {
                return creatable;
            }

            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Creatable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(user, Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
                    if (authorized)
                    {
                        creatable.Add(ctd);
                    }
                }
            }
            return creatable;
        }

        public async Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync(ClaimsPrincipal user)
        {
            var listable = new List<ContentTypeDefinition>();

            if (user == null)
            {
                return listable;
            }

            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Listable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(user, Permissions.EditContent, await _contentManager.NewAsync(ctd.Name));
                    if (authorized)
                    {
                        listable.Add(ctd);
                    }
                }
            }
            return listable;

        }

    }
}
