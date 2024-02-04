using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Azure.Email.Drivers;

public class AzureEmailSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureEmailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AzureEmailSettings _azureEmailSettings;
    private readonly IAuthorizationService _authorizationService;

    public AzureEmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AzureEmailSettings> azureEmailSettings,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _azureEmailSettings = azureEmailSettings.Value;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(AzureEmailSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
        {
            return null;
        }

        return Initialize<AzureEmailSettingsViewModel>("AzureEmailSettings_Edit", model =>
        {
            model.DefaultSender = _azureEmailSettings.DefaultSender;
            model.IsConfigured = !string.IsNullOrWhiteSpace(_azureEmailSettings.ConnectionString);
        }).Location("Content:5#Azure")
        .OnGroup(EmailSettings.GroupId);
    }
}
