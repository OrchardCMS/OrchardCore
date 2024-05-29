using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Scripting;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers
{
    public class ScriptExternalLoginEventHandler : IExternalLoginEventHandler
    {
        private readonly ILogger _logger;
        private readonly IScriptingManager _scriptingManager;
        private readonly ISiteService _siteService;
        private static readonly JsonMergeSettings _jsonMergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Merge
        };

        public ScriptExternalLoginEventHandler(
            ISiteService siteService,
            IScriptingManager scriptingManager,
            ILogger<ScriptExternalLoginEventHandler> logger
        )
        {
            _siteService = siteService;
            _scriptingManager = scriptingManager;
            _logger = logger;
        }

        public async Task<string> GenerateUserName(string provider, IEnumerable<SerializableClaim> claims)
        {
            var registrationSettings = await _siteService.GetSettingsAsync<RegistrationSettings>();

            if (registrationSettings.UseScriptToGenerateUsername)
            {
                var context = new { userName = string.Empty, loginProvider = provider, externalClaims = claims };

                var script = $"js: function generateUsername(context) {{\n{registrationSettings.GenerateUsernameScript}\n}}\nvar context = {JConvert.SerializeObject(context, JOptions.CamelCase)};\ngenerateUsername(context);\nreturn context;";

                dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                if (evaluationResult?.userName != null)
                {
                    return evaluationResult.userName;
                }
            }
            return string.Empty;
        }

        public async Task UpdateUserAsync(UpdateUserContext context)
        {
            var loginSettings = await _siteService.GetSettingsAsync<LoginSettings>();
            
            UpdateUserInternal(context, loginSettings);
        }

        public void UpdateUserInternal(UpdateUserContext context, LoginSettings loginSettings)
        {
            if (loginSettings.UseScriptToSyncRoles)
            {
                var script = $"js: function syncRoles(context) {{\n{loginSettings.SyncRolesScript}\n}}\nvar context={JConvert.SerializeObject(context, JOptions.CamelCase)};\nsyncRoles(context);\nreturn context;";
                dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                context.RolesToAdd.AddRange((evaluationResult.rolesToAdd as object[]).Select(i => i.ToString()));
                context.RolesToRemove.AddRange((evaluationResult.rolesToRemove as object[]).Select(i => i.ToString()));

                if (evaluationResult.claimsToUpdate is not null)
                {
                    var claimsToUpdate = ((JsonArray)JArray.FromObject(evaluationResult.claimsToUpdate)).Deserialize<List<UserClaim>>(JOptions.CamelCase);
                    context.ClaimsToUpdate.AddRange(claimsToUpdate);
                }

                if (evaluationResult.claimsToRemove is not null)
                {
                    var claimsToRemove = ((JsonArray)JArray.FromObject(evaluationResult.claimsToRemove)).Deserialize<List<UserClaim>>(JOptions.CamelCase);
                    context.ClaimsToRemove.AddRange(claimsToRemove);
                }

                if (evaluationResult.propertiesToUpdate is not null)
                {
                    var result = (JsonObject)JObject.FromObject(evaluationResult.propertiesToUpdate);
                    if (context.PropertiesToUpdate is not null)
                    {
                        // Perhaps other provider will fill some values. we should keep exists value.
                        context.PropertiesToUpdate.Merge(result, _jsonMergeSettings);
                    }
                    else
                    {
                        context.PropertiesToUpdate = result;
                    }
                }
            }
        }
    }
}
