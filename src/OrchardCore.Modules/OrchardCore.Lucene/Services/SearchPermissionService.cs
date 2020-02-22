using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;
using OrchardCore.Users.Services;

namespace OrchardCore.Lucene.Services
{
    public class SearchPermissionService : ISearchPermissionService
    {
        private readonly IMembershipService _membershipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ILogger<SearchPermissionService> _logger;
        public SearchPermissionService(
            IMembershipService membershipService,
            IAuthorizationService authorizationService,
            LuceneIndexManager luceneIndexProvider,
            IEnumerable<IPermissionProvider> permissionProviders,
            ILogger<SearchPermissionService> logger,
            IStringLocalizer<SearchPermissionService> S
            )
        {
            _membershipService = membershipService;
            _authorizationService = authorizationService;
            _luceneIndexProvider = luceneIndexProvider;
            _permissionProviders = permissionProviders;
            _logger = logger;
            this.S = S;
        }

        public IStringLocalizer S { get; }
        public async Task<SearchPermissionResult> CheckPermission(string indexName, ClaimsPrincipal user)
        {
            var permissionsProvider = _permissionProviders.FirstOrDefault(x => x.GetType().FullName == "OrchardCore.Lucene.Permissions");
            var permissions = await permissionsProvider.GetPermissionsAsync();

            if (permissions.FirstOrDefault(x => x.Name == "QueryLucene" + indexName + "Index") != null)
            {
                if (!await _authorizationService.AuthorizeAsync(user, permissions.FirstOrDefault(x => x.Name == "QueryLucene" + indexName + "Index")))
                {

                    _logger.LogInformation($"User {user.Identity.Name} do not have permission to execute search.");
                    return SearchPermissionResult.Forbid();
                }
            }
            else
            {
                _logger.LogInformation("Couldn't execute search. The search index doesn't exist.");
                return SearchPermissionResult.Fail(S["Search is not configured."]);
            }

            if (!string.IsNullOrEmpty(indexName) && !_luceneIndexProvider.Exists(indexName))
            {
                _logger.LogInformation("Couldn't execute search. The search index doesn't exist.");
                return SearchPermissionResult.Fail(S["Search is not configured."]);
            }

            return SearchPermissionResult.Success;
        }

        public async Task<SearchPermissionResult> CheckPermission(string indexName, IUser user)
        {
            var userClaim = await _membershipService.CreateClaimsPrincipal(user);
            return await CheckPermission(indexName, userClaim);
        }

    }
}
