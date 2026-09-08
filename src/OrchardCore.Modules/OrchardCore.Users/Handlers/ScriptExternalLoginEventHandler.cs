using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.Extensions.Logging;
using OrchardCore.Scripting;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

public class ScriptExternalLoginEventHandler : IExternalLoginEventHandler
{
    // The scripts are prepared and cached by their source text, so the per-user state must be handed to
    // the script through a global method rather than being serialized into the script itself. Otherwise
    // every external login would add a distinct entry to the shared cache.
    private const string ContextMethodName = "externalLoginContext";

    private readonly ILogger _logger;
    private readonly IScriptingManager _scriptingManager;
    private readonly ISiteService _siteService;
    private static readonly JsonMergeSettings s_jsonMergeSettings = new JsonMergeSettings
    {
        MergeArrayHandling = MergeArrayHandling.Union,
        MergeNullValueHandling = MergeNullValueHandling.Merge,
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
        var registrationSettings = await _siteService.GetSettingsAsync<ExternalRegistrationSettings>();

        if (registrationSettings.UseScriptToGenerateUsername)
        {
            var context = new
            {
                userName = string.Empty,
                loginProvider = provider,
                externalClaims = claims,
            };

            var script = $"js: function generateUsername(context) {{\n{registrationSettings.GenerateUsernameScript}\n}}\nvar context = JSON.parse({ContextMethodName}());\ngenerateUsername(context);\nreturn context;";

            dynamic evaluationResult = await _scriptingManager.EvaluateAsync(script, null, null, [CreateContextMethodProvider(context)]);

            if (evaluationResult is IDictionary<string, object> data && data.TryGetValue("userName", out var userNameObj))
            {
                return (string)userNameObj;
            }
        }

        return string.Empty;
    }

    public async Task UpdateUserAsync(UpdateUserContext context)
    {
        var loginSettings = await _siteService.GetSettingsAsync<ExternalLoginSettings>();

        await UpdateUserInternalAsync(context, loginSettings);
    }

    public async Task UpdateUserInternalAsync(UpdateUserContext context, ExternalLoginSettings loginSettings)
    {
        if (!loginSettings.UseScriptToSyncProperties)
        {
            return;
        }

        var script = $"js: function syncRoles(context) {{\n{loginSettings.SyncPropertiesScript}\n}}\nvar context = JSON.parse({ContextMethodName}());\nsyncRoles(context);\nreturn context;";
        dynamic evaluationResult = await _scriptingManager.EvaluateAsync(script, null, null, [CreateContextMethodProvider(context)]);
        context.RolesToAdd.AddRange((evaluationResult.rolesToAdd as object[]).Select(i => i.ToString()));
        context.RolesToRemove.AddRange((evaluationResult.rolesToRemove as object[]).Select(i => i.ToString()));

        if (evaluationResult is IDictionary<string, object> data)
        {
            if (data.TryGetValue("claimsToUpdate", out var claimsToUpdateObj))
            {
                var claimsToUpdate = JArray.FromObject(claimsToUpdateObj).Deserialize<List<UserClaim>>(JOptions.CamelCase);
                context.ClaimsToUpdate.AddRange(claimsToUpdate);
            }

            if (data.TryGetValue("claimsToRemove", out var claimsToRemoveObj))
            {
                var claimsToRemove = JArray.FromObject(claimsToRemoveObj).Deserialize<List<UserClaim>>(JOptions.CamelCase);
                context.ClaimsToRemove.AddRange(claimsToRemove);
            }

            if (data.TryGetValue("propertiesToUpdate", out var propertiesToUpdateObj))
            {
                var result = JObject.FromObject(propertiesToUpdateObj);
                if (context.PropertiesToUpdate is not null)
                {
                    // Perhaps other provider will fill some values. we should keep exists value.
                    context.PropertiesToUpdate.Merge(result, s_jsonMergeSettings);
                }
                else
                {
                    context.PropertiesToUpdate = result;
                }
            }
        }
    }

    private static ContextMethodProvider CreateContextMethodProvider(object context)
        => new(JConvert.SerializeObject(context, JOptions.CamelCase));

    private sealed class ContextMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _globalMethod;

        public ContextMethodProvider(string serializedContext)
        {
            _globalMethod = new GlobalMethod
            {
                Name = ContextMethodName,
                Method = _ => (Func<string>)(() => serializedContext),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods() => [_globalMethod];
    }
}
