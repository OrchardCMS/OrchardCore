using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Deployment.Indexes;
using YesSql;

namespace OrchardCore.Deployment
{
    public class DeploymentPlanService
    {
        private readonly YesSql.ISession _session;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly Lazy<Dictionary<string, DeploymentPlan>> _deploymentPlans;

        public DeploymentPlanService(
            YesSql.ISession session,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _deploymentPlans = new Lazy<Dictionary<string, DeploymentPlan>>(() =>
                {
                    var deploymentPlanQuery = _session.Query<DeploymentPlan, DeploymentPlanIndex>();
                    var deploymentPlans = deploymentPlanQuery.ListAsync().Result;
                    return deploymentPlans.ToDictionary(x => x.Name);
                });
        }

        public async Task<bool> DoesUserHavePermissionsAsync()
        {
            var user = _httpContextAccessor.HttpContext.User;

            var result = await _authorizationService.AuthorizeAsync(user, Permissions.ManageDeploymentPlan) &&
                         await _authorizationService.AuthorizeAsync(user, Permissions.Export);

            return result;
        }

        public async Task<IEnumerable<string>> GetAllDeploymentPlanNamesAsync()
        {
            var deploymentPlans = await _deploymentPlans.Value;

            return deploymentPlans.Keys;
        }

        public async Task<IEnumerable<DeploymentPlan>> GetAllDeploymentPlansAsync()
        {
            var deploymentPlans = await _deploymentPlans.Value;

            return deploymentPlans.Values;
        }

        public async Task<IEnumerable<DeploymentPlan>> GetDeploymentPlansAsync(params string[] deploymentPlanNames)
        {
            var deploymentPlans = await _deploymentPlans.Value;

            return GetDeploymentPlans(deploymentPlans, deploymentPlanNames);
        }

        private static IEnumerable<DeploymentPlan> GetDeploymentPlans(IDictionary<string, DeploymentPlan> deploymentPlans, params string[] deploymentPlanNames)
        {
            foreach (var deploymentPlanName in deploymentPlanNames)
            {
                DeploymentPlan deploymentPlan;
                if (deploymentPlans.TryGetValue(deploymentPlanName, out deploymentPlan))
                {
                    yield return deploymentPlan;
                }
            }
        }

        public async Task CreateOrUpdateDeploymentPlans(IEnumerable<DeploymentPlan> deploymentPlans)
        {
            foreach (var deploymentPlan in deploymentPlans)
            {
                _session.Save(deploymentPlan);
            }
        }
    }
}