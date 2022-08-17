using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Security.Services;

namespace OrchardCore.OpenId.Controllers
{
    [Admin, Feature(OpenIdConstants.Features.Management)]
    public class ApplicationController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly PagerOptions _pagerOptions;
        private readonly IOpenIdApplicationManager _applicationManager;
        private readonly IOpenIdScopeManager _scopeManager;
        private readonly INotifier _notifier;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly dynamic New;

        public ApplicationController(
            IShapeFactory shapeFactory,
            IOptions<PagerOptions> pagerOptions,
            IStringLocalizer<ApplicationController> stringLocalizer,
            IAuthorizationService authorizationService,
            IOpenIdApplicationManager applicationManager,
            IOpenIdScopeManager scopeManager,
            IHtmlLocalizer<ApplicationController> htmlLocalizer,
            INotifier notifier,
            ShellDescriptor shellDescriptor)
        {
            New = shapeFactory;
            _pagerOptions = pagerOptions.Value;
            S = stringLocalizer;
            H = htmlLocalizer;
            _authorizationService = authorizationService;
            _applicationManager = applicationManager;
            _scopeManager = scopeManager;
            _notifier = notifier;
            _shellDescriptor = shellDescriptor;
        }

        public async Task<ActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageApplications))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);
            var count = await _applicationManager.CountAsync();

            var model = new OpenIdApplicationsIndexViewModel
            {
                Pager = (await New.Pager(pager)).TotalItemCount(count)
            };

            await foreach (var application in _applicationManager.ListAsync(pager.PageSize, pager.GetStartIndex()))
            {
                model.Applications.Add(new OpenIdApplicationEntry
                {
                    DisplayName = await _applicationManager.GetDisplayNameAsync(application),
                    Id = await _applicationManager.GetPhysicalIdAsync(application)
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageApplications))
            {
                return Forbid();
            }

            var model = new CreateOpenIdApplicationViewModel();

            var roleService = HttpContext.RequestServices?.GetService<IRoleService>();
            if (roleService != null)
            {
                foreach (var role in await roleService.GetRoleNamesAsync())
                {
                    model.RoleEntries.Add(new CreateOpenIdApplicationViewModel.RoleEntry
                    {
                        Name = role
                    });
                }
            }
            else
            {
                await _notifier.WarningAsync(H["There are no registered services to provide roles."]);
            }

            await foreach (var scope in _scopeManager.ListAsync(null, null, default))
            {
                model.ScopeEntries.Add(new CreateOpenIdApplicationViewModel.ScopeEntry
                {
                    Name = await _scopeManager.GetNameAsync(scope)
                });
            }

            ViewData[nameof(OpenIdServerSettings)] = await GetServerSettingsAsync();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageApplications))
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(model.ClientSecret) &&
                 string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.ClientSecret), S["No client secret can be set for public applications."]);
            }
            else if (string.IsNullOrEmpty(model.ClientSecret) &&
                     string.Equals(model.Type, OpenIddictConstants.ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.ClientSecret), S["The client secret is required for confidential applications."]);
            }

            if (!string.IsNullOrEmpty(model.ClientId) && await _applicationManager.FindByClientIdAsync(model.ClientId) != null)
            {
                ModelState.AddModelError(nameof(model.ClientId), S["The client identifier is already taken by another application."]);
            }

            if (!ModelState.IsValid)
            {
                ViewData[nameof(OpenIdServerSettings)] = await GetServerSettingsAsync();
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var settings = new OpenIdApplicationSettings()
            {
                AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow,
                AllowClientCredentialsFlow = model.AllowClientCredentialsFlow,
                AllowHybridFlow = model.AllowHybridFlow,
                AllowImplicitFlow = model.AllowImplicitFlow,
                AllowIntrospectionEndpoint = model.AllowIntrospectionEndpoint,
                AllowLogoutEndpoint = model.AllowLogoutEndpoint,
                AllowPasswordFlow = model.AllowPasswordFlow,
                AllowRefreshTokenFlow = model.AllowRefreshTokenFlow,
                AllowRevocationEndpoint = model.AllowRevocationEndpoint,
                ClientId = model.ClientId,
                ClientSecret = model.ClientSecret,
                ConsentType = model.ConsentType,
                DisplayName = model.DisplayName,
                PostLogoutRedirectUris = model.PostLogoutRedirectUris,
                RedirectUris = model.RedirectUris,
                Roles = model.RoleEntries.Where(x => x.Selected).Select(x => x.Name).ToArray(),
                Scopes = model.ScopeEntries.Where(x => x.Selected).Select(x => x.Name).ToArray(),
                Type = model.Type,
                RequireProofKeyForCodeExchange = model.RequireProofKeyForCodeExchange
            };

            await _applicationManager.UpdateDescriptorFromSettings(settings);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction(nameof(Index));
            }

            return this.LocalRedirect(returnUrl, true);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageApplications))
            {
                return Forbid();
            }

            var application = await _applicationManager.FindByPhysicalIdAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            ValueTask<bool> HasPermissionAsync(string permission) => _applicationManager.HasPermissionAsync(application, permission);
            ValueTask<bool> HasRequirementAsync(string requirement) => _applicationManager.HasRequirementAsync(application, requirement);

            var model = new EditOpenIdApplicationViewModel
            {
                AllowAuthorizationCodeFlow = await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode) &&
                                             await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.Code),

                AllowClientCredentialsFlow = await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials),

                // Note: the hybrid flow doesn't have a dedicated grant_type but is treated as a combination
                // of both the authorization code and implicit grants. As such, to determine whether the hybrid
                // flow is enabled, both the authorization code grant and the implicit grant MUST be enabled.
                AllowHybridFlow = await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode) &&
                                  await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.Implicit) &&
                                  (await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken) ||
                                   await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken) ||
                                   await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.CodeToken)),

                AllowImplicitFlow = await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.Implicit) &&
                                    (await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.IdToken) ||
                                     await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken) ||
                                     await HasPermissionAsync(OpenIddictConstants.Permissions.ResponseTypes.Token)),

                AllowPasswordFlow = await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.Password),
                AllowRefreshTokenFlow = await HasPermissionAsync(OpenIddictConstants.Permissions.GrantTypes.RefreshToken),
                AllowLogoutEndpoint = await HasPermissionAsync(OpenIddictConstants.Permissions.Endpoints.Logout),
                AllowIntrospectionEndpoint = await HasPermissionAsync(OpenIddictConstants.Permissions.Endpoints.Introspection),
                AllowRevocationEndpoint = await HasPermissionAsync(OpenIddictConstants.Permissions.Endpoints.Revocation),
                ClientId = await _applicationManager.GetClientIdAsync(application),
                ConsentType = await _applicationManager.GetConsentTypeAsync(application),
                DisplayName = await _applicationManager.GetDisplayNameAsync(application),
                Id = await _applicationManager.GetPhysicalIdAsync(application),
                PostLogoutRedirectUris = string.Join(" ", await _applicationManager.GetPostLogoutRedirectUrisAsync(application)),
                RedirectUris = string.Join(" ", await _applicationManager.GetRedirectUrisAsync(application)),
                Type = await _applicationManager.GetClientTypeAsync(application),
                RequireProofKeyForCodeExchange = await HasRequirementAsync(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange)
            };

            var roleService = HttpContext.RequestServices?.GetService<IRoleService>();
            if (roleService != null)
            {
                var roles = await _applicationManager.GetRolesAsync(application);

                foreach (var role in await roleService.GetRoleNamesAsync())
                {
                    model.RoleEntries.Add(new EditOpenIdApplicationViewModel.RoleEntry
                    {
                        Name = role,
                        Selected = roles.Contains(role, StringComparer.OrdinalIgnoreCase)
                    });
                }
            }
            else
            {
                await _notifier.WarningAsync(H["There are no registered services to provide roles."]);
            }

            var permissions = await _applicationManager.GetPermissionsAsync(application);
            await foreach (var scope in _scopeManager.ListAsync())
            {
                var scopeName = await _scopeManager.GetNameAsync(scope);
                model.ScopeEntries.Add(new EditOpenIdApplicationViewModel.ScopeEntry
                {
                    Name = scopeName,
                    Selected = await _applicationManager.HasPermissionAsync(application, OpenIddictConstants.Permissions.Prefixes.Scope + scopeName)
                });
            }

            ViewData[nameof(OpenIdServerSettings)] = await GetServerSettingsAsync();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditOpenIdApplicationViewModel model, string returnUrl = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageApplications))
            {
                return Forbid();
            }

            var application = await _applicationManager.FindByPhysicalIdAsync(model.Id);
            if (application == null)
            {
                return NotFound();
            }

            // If the application was a public client and is now a confidential client, ensure a client secret was provided.
            if (string.IsNullOrEmpty(model.ClientSecret) &&
               !string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase) &&
                await _applicationManager.HasClientTypeAsync(application, OpenIddictConstants.ClientTypes.Public))
            {
                ModelState.AddModelError(nameof(model.ClientSecret), S["Setting a new client secret is required."]);
            }

            if (!string.IsNullOrEmpty(model.ClientSecret) &&
                 string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.ClientSecret), S["No client secret can be set for public applications."]);
            }

            if (ModelState.IsValid)
            {
                var other = await _applicationManager.FindByClientIdAsync(model.ClientId);
                if (other != null && !string.Equals(
                    await _applicationManager.GetIdAsync(other),
                    await _applicationManager.GetIdAsync(application), StringComparison.Ordinal))
                {
                    ModelState.AddModelError(nameof(model.ClientId), S["The client identifier is already taken by another application."]);
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData[nameof(OpenIdServerSettings)] = await GetServerSettingsAsync();
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var settings = new OpenIdApplicationSettings()
            {
                AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow,
                AllowClientCredentialsFlow = model.AllowClientCredentialsFlow,
                AllowHybridFlow = model.AllowHybridFlow,
                AllowImplicitFlow = model.AllowImplicitFlow,
                AllowIntrospectionEndpoint = model.AllowIntrospectionEndpoint,
                AllowLogoutEndpoint = model.AllowLogoutEndpoint,
                AllowPasswordFlow = model.AllowPasswordFlow,
                AllowRefreshTokenFlow = model.AllowRefreshTokenFlow,
                AllowRevocationEndpoint = model.AllowRevocationEndpoint,
                ClientId = model.ClientId,
                ClientSecret = model.ClientSecret,
                ConsentType = model.ConsentType,
                DisplayName = model.DisplayName,
                PostLogoutRedirectUris = model.PostLogoutRedirectUris,
                RedirectUris = model.RedirectUris,
                Roles = model.RoleEntries.Where(x => x.Selected).Select(x => x.Name).ToArray(),
                Scopes = model.ScopeEntries.Where(x => x.Selected).Select(x => x.Name).ToArray(),
                Type = model.Type,
                RequireProofKeyForCodeExchange = model.RequireProofKeyForCodeExchange
            };

            await _applicationManager.UpdateDescriptorFromSettings(settings, application);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction(nameof(Index));
            }

            return this.LocalRedirect(returnUrl, true);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageApplications))
            {
                return Forbid();
            }

            var application = await _applicationManager.FindByPhysicalIdAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            await _applicationManager.DeleteAsync(application);

            return RedirectToAction(nameof(Index));
        }

        private async Task<OpenIdServerSettings> GetServerSettingsAsync()
        {
            if (_shellDescriptor.Features.Any(feature => feature.Id == OpenIdConstants.Features.Server))
            {
                var service = HttpContext.RequestServices.GetRequiredService<IOpenIdServerService>();
                var settings = await service.GetSettingsAsync();
                if ((await service.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
                {
                    await _notifier.WarningAsync(H["OpenID Connect settings are not properly configured."]);
                }

                return settings;
            }

            return null;
        }
    }
}
