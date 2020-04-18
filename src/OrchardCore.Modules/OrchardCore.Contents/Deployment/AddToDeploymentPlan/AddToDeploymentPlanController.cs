using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Deployment.AddToDeploymentPlan
{
    [Feature("OrchardCore.Contents.AddToDeploymentPlan")]
    [Admin]
    public class AddToDeploymentPlanController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IEnumerable<IDeploymentStepFactory> _factories;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;

        public AddToDeploymentPlanController(
            IAuthorizationService authorizationService,
            IContentManager contentManager, ISession session,
            IEnumerable<IDeploymentStepFactory> factories,
            INotifier notifier,
            IHtmlLocalizer<AddToDeploymentPlanController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _session = session;
            _factories = factories;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        [HttpPost]
        public async Task<IActionResult> AddContentItem(int deploymentPlanId, string returnUrl, string contentItemId, bool latest)
        {
            if (!(await _authorizationService.AuthorizeAsync(User, OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan) &&
                await _authorizationService.AuthorizeAsync(User, OrchardCore.Deployment.CommonPermissions.Export)
                ))
            {
                return Forbid();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(deploymentPlanId);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, latest == false ? VersionOptions.Published : VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            // Export permission is required as the overriding permission.
            // Requesting EditContent would allow custom permissions to deny access to this content item.
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var step = (ContentItemDeploymentStep)_factories.FirstOrDefault(x => x.Name == nameof(ContentItemDeploymentStep)).Create();
            step.ContentItem = JObject.FromObject(contentItem);

            deploymentPlan.DeploymentSteps.Add(step);

            _notifier.Success(H["Content successfully added to deployment plan."]);

            _session.Save(deploymentPlan);

            return LocalRedirect(returnUrl);
        }


        [HttpPost]
        public async Task<IActionResult> AddContentItems(int deploymentPlanId, string returnUrl, IEnumerable<int> itemIds)
        {
            if (itemIds?.Count() == 0)
            {
                return LocalRedirect(returnUrl);
            }

            if (!(await _authorizationService.AuthorizeAsync(User, OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan) &&
                await _authorizationService.AuthorizeAsync(User, OrchardCore.Deployment.CommonPermissions.Export)
                ))
            {
                return Forbid();
            }

            var deploymentPlan = await _session.GetAsync<DeploymentPlan>(deploymentPlanId);

            if (deploymentPlan == null)
            {
                return NotFound();
            }

            var contentItems = await _session.Query<ContentItem, ContentItemIndex>().Where(x => x.DocumentId.IsIn(itemIds) && x.Published).ListAsync();

            foreach (var item in contentItems)
            {
                // Export permission is required as the overriding permission.
                // Requesting EditContent would allow custom permissions to deny access to this content item.
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, item))
                {
                    _notifier.Warning(H["Couldn't add selected content to deployment plan."]);
                    _session.Cancel();

                    return Forbid();
                }
                var step = (ContentItemDeploymentStep)_factories.FirstOrDefault(x => x.Name == nameof(ContentItemDeploymentStep)).Create();
                step.ContentItem = JObject.FromObject(item);

                deploymentPlan.DeploymentSteps.Add(step);
            }

            _notifier.Success(H["Content successfully added to deployment plan."]);

            _session.Save(deploymentPlan);

            return LocalRedirect(returnUrl);
        }
    }
}
