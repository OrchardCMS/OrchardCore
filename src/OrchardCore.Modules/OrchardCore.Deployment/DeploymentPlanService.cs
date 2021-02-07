using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Deployment.Indexes;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Deployment
{
    public class DeploymentPlanService : IDeploymentPlanService
    {
        private readonly YesSql.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private Dictionary<string, DeploymentPlan> _deploymentPlans;

        public DeploymentPlanService(
            YesSql.ISession session,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        private async Task<Dictionary<string, DeploymentPlan>> GetDeploymentPlans()
        {
            if (_deploymentPlans == null)
            {
                var deploymentPlanQuery = _session.Query<DeploymentPlan, DeploymentPlanIndex>();
                var deploymentPlans = await deploymentPlanQuery.ListAsync();
                _deploymentPlans = deploymentPlans.ToDictionary(x => x.Name);
            }

            return _deploymentPlans;
        }

        public async Task<bool> DoesUserHavePermissionsAsync()
        {
            var user = _httpContextAccessor.HttpContext.User;

            var result = await _authorizationService.AuthorizeAsync(user, Permissions.ManageDeploymentPlan) &&
                         await _authorizationService.AuthorizeAsync(user, Permissions.Export);

            return result;
        }

        public async Task<bool> DoesUserHaveExportPermissionAsync()
        {
            var user = _httpContextAccessor.HttpContext.User;

            var result = await _authorizationService.AuthorizeAsync(user, Permissions.Export);

            return result;
        }

        public async Task<IEnumerable<string>> GetAllDeploymentPlanNamesAsync()
        {
            var deploymentPlans = await GetDeploymentPlans();

            return deploymentPlans.Keys;
        }

        public async Task<IEnumerable<DeploymentPlan>> GetAllDeploymentPlansAsync()
        {
            var deploymentPlans = await GetDeploymentPlans();

            return deploymentPlans.Values;
        }

        public async Task<IEnumerable<DeploymentPlan>> GetDeploymentPlansAsync(params string[] deploymentPlanNames)
        {
            var deploymentPlans = await GetDeploymentPlans();

            return GetDeploymentPlans(deploymentPlans, deploymentPlanNames);
        }

        private static IEnumerable<DeploymentPlan> GetDeploymentPlans(IDictionary<string, DeploymentPlan> deploymentPlans, params string[] deploymentPlanNames)
        {
            foreach (var deploymentPlanName in deploymentPlanNames)
            {
                if (deploymentPlans.TryGetValue(deploymentPlanName, out var deploymentPlan))
                {
                    yield return deploymentPlan;
                }
            }
        }

        public async Task CreateOrUpdateDeploymentPlansAsync(IEnumerable<DeploymentPlan> deploymentPlans)
        {
            var names = deploymentPlans.Select(x => x.Name);

            var existingDeploymentPlans = (await _session.Query<DeploymentPlan, DeploymentPlanIndex>(x => x.Name.IsIn(names))
                .ListAsync())
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var deploymentPlan in deploymentPlans)
            {
                if (existingDeploymentPlans.TryGetValue(deploymentPlan.Name, out var existingDeploymentPlan))
                {
                    existingDeploymentPlan.Name = deploymentPlan.Name;
                    existingDeploymentPlan.DeploymentSteps.Clear();
                    existingDeploymentPlan.DeploymentSteps.AddRange(deploymentPlan.DeploymentSteps);

                    _session.Save(existingDeploymentPlan);
                }
                else
                {
                    _session.Save(deploymentPlan);
                }
            }
        }
    }
}
