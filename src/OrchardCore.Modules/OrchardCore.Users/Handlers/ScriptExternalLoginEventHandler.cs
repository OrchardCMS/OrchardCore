using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.Entities;
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

        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
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
            var registrationSettings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (registrationSettings.UseScriptToGenerateUsername)
            {
                var context = new { userName = String.Empty, loginProvider = provider, externalClaims = claims };

                var script = $"js: function generateUsername(context) {{\n{registrationSettings.GenerateUsernameScript}\n}}\nvar context = {JsonConvert.SerializeObject(context, _jsonSettings)};\ngenerateUsername(context);\nreturn context;";

                dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                if (evaluationResult?.userName != null)
                {
                    return evaluationResult.userName;
                }
            }
            return String.Empty;
        }

        public async Task UpdateRoles(UpdateRolesContext context)
        {
            var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();
            if (loginSettings.UseScriptToSyncRoles)
            {
                var script = $"js: function syncRoles(context) {{\n{loginSettings.SyncRolesScript}\n}}\nvar context={JsonConvert.SerializeObject(context, _jsonSettings)};\nsyncRoles(context);\nreturn context;";
                dynamic evaluationResult = _scriptingManager.Evaluate(script, null, null, null);
                context.RolesToAdd.AddRange((evaluationResult.rolesToAdd as object[]).Select(i => i.ToString()));
                context.RolesToRemove.AddRange((evaluationResult.rolesToRemove as object[]).Select(i => i.ToString()));
            }
        }
    }
}
