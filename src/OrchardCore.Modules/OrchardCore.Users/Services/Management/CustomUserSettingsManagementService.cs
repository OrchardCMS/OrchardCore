using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Json;
using OrchardCore.Security;
using OrchardCore.Users.Models;
using ISession = YesSql.ISession;

namespace OrchardCore.Users.Services;

internal sealed class CustomUserSettingsManagementService
{
    private readonly CustomUserSettingsService _settings;
    private readonly UserManager<IUser> _users;
    private readonly IAuthorizationService _authorization;
    private readonly IContentManager _content;
    private readonly ISession _session;
    private readonly ContentOptions _contentOptions;
    private readonly JsonSerializerOptions _serializerOptions;

    public CustomUserSettingsManagementService(CustomUserSettingsService settings, UserManager<IUser> users,
        IAuthorizationService authorization, IContentManager content, ISession session,
        IOptions<ContentOptions> contentOptions, IOptions<DocumentJsonSerializerOptions> serializerOptions)
    {
        _settings = settings;
        _users = users;
        _authorization = authorization;
        _content = content;
        _session = session;
        _contentOptions = contentOptions.Value;
        _serializerOptions = serializerOptions.Value.SerializerOptions;
    }

    public async Task<IResult> ListAsync(ClaimsPrincipal principal)
    {
        var items = new JsonArray();
        foreach (var type in (await _settings.GetAllSettingsTypesAsync()).OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            if (await CanManageTypeAsync(principal, type))
            {
                items.Add(new JsonObject { ["name"] = type.Name, ["displayName"] = type.DisplayName });
            }
        }

        return Results.Json(new JsonObject { ["items"] = items });
    }

    public async Task<IResult> SchemaAsync(ClaimsPrincipal principal, string name)
    {
        var type = await _settings.GetSettingsTypeAsync(name);
        if (type is null || !await CanManageTypeAsync(principal, type))
        {
            return Results.NotFound();
        }

        return Results.Json(EmbeddedContentItemApi.BuildSchema(type, _contentOptions, _serializerOptions), _serializerOptions);
    }

    public async Task<IResult> GetAsync(ClaimsPrincipal principal, string userId, string name)
    {
        var type = await _settings.GetSettingsTypeAsync(name);
        if (type is null || !await CanManageTypeAsync(principal, type) || await _users.FindByIdAsync(userId) is not User user)
        {
            return Results.NotFound();
        }

        if (!await _authorization.AuthorizeAsync(principal, UsersPermissions.ViewUsers, user))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        return Results.Json(Envelope(await _settings.GetSettingsAsync(user, type), type), _serializerOptions);
    }

    public async Task<IResult> UpdateAsync(ClaimsPrincipal principal, string userId, string name, JsonObject input)
    {
        var type = await _settings.GetSettingsTypeAsync(name);
        if (type is null || !await CanManageTypeAsync(principal, type) || await _users.FindByIdAsync(userId) is not User user)
        {
            return Results.NotFound();
        }

        if (!await _authorization.AuthorizeAsync(principal, UsersPermissions.EditUsers, user))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var errors = EmbeddedContentItemApi.ValidateInput(type.Name, type, input);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var item = await _settings.GetSettingsAsync(user, type);
        var previous = Envelope(item, type);
        try
        {
            item.Merge(input, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
            });
            item.ContentType = type.Name;
            errors = EmbeddedContentItemApi.ValidateEnvelope(type, Envelope(item, type));
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            if (JsonNode.DeepEquals(previous, Envelope(item, type)))
            {
                return Results.Json(previous, _serializerOptions);
            }

            await _content.UpdateAsync(item);
            var validation = await _content.ValidateAsync(item);
            if (!validation.Succeeded)
            {
                await _session.CancelAsync();
                return Results.ValidationProblem(EmbeddedContentItemApi.CreateValidationErrors(validation));
            }
        }
        catch (JsonException)
        {
            await _session.CancelAsync();
            return Results.ValidationProblem(new Dictionary<string, string[]> { [string.Empty] = ["The settings payload contains an invalid value."] });
        }

        // The content pipeline operates on a detached item. Only a valid result reaches its owner.
        var previousProperties = user.Properties.DeepClone().AsObject();
        CustomUserSettingsService.SetSettings(user, type, item);
        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            user.Properties = previousProperties;
            await _session.CancelAsync();
            return Results.ValidationProblem(new Dictionary<string, string[]> { [string.Empty] = result.Errors.Select(error => error.Description).ToArray() });
        }

        _session.Delete(item);
        return Results.Json(Envelope(item, type), _serializerOptions);
    }

    private Task<bool> CanManageTypeAsync(ClaimsPrincipal principal, ContentTypeDefinition type)
        => _authorization.AuthorizeAsync(principal, CustomUserSettingsPermissions.CreatePermissionForType(type));

    private JsonObject Envelope(ContentItem item, ContentTypeDefinition type)
        => EmbeddedContentItemApi.CreateSafeEnvelope(item, type, _serializerOptions);
}
