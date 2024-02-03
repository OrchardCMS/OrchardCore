using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Drivers;

public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpEmailSettings>
{
    public const string GroupId = "smtp-email";
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly SmtpEmailSettings _smtpEmailSettings;

    public SmtpSettingsDisplayDriver(
        IDataProtectionProvider dataProtectionProvider,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<SmtpEmailSettings> smtpEmailSettings)
    {
        _dataProtectionProvider = dataProtectionProvider;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _smtpEmailSettings = smtpEmailSettings.Value;
    }

    public override async Task<IDisplayResult> EditAsync(SmtpEmailSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSmtpEmailSettings))
        {
            return null;
        }

        var shapes = new List<IDisplayResult>
        {
            Initialize<SmtpEmailSettings>("SmtpEmailSettings_Edit", model =>
            {
                model.DefaultSender = _smtpEmailSettings.DefaultSender;
                model.DeliveryMethod = settings.DeliveryMethod;
                model.PickupDirectoryLocation = settings.PickupDirectoryLocation;
                model.Host = settings.Host;
                model.Port = settings.Port;
                model.ProxyHost = settings.ProxyHost;
                model.ProxyPort = settings.ProxyPort;
                model.EncryptionMethod = settings.EncryptionMethod;
                model.AutoSelectEncryption = settings.AutoSelectEncryption;
                model.RequireCredentials = settings.RequireCredentials;
                model.UseDefaultCredentials = settings.UseDefaultCredentials;
                model.UserName = settings.UserName;
                model.Password = settings.Password;
                model.IgnoreInvalidSslCertificate = settings.IgnoreInvalidSslCertificate;
            }).Location("Content:5").OnGroup(GroupId),
        };

        if (_smtpEmailSettings.DefaultSender != null)
        {
            shapes.Add(Dynamic("SmtpEmailSettings_TestButton").Location("Actions").OnGroup(GroupId));
        }

        return Combine(shapes);
    }

    public override async Task<IDisplayResult> UpdateAsync(SmtpEmailSettings section, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSmtpEmailSettings))
        {
            return null;
        }

        if (!context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        var previousPassword = section.Password;
        // Restore password if the input is empty, meaning that it has not been reset.
        if (string.IsNullOrWhiteSpace(section.Password))
        {
            section.Password = previousPassword;
        }
        else
        {
            // Encrypt the password.
            var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpEmailSettingsConfiguration));
            section.Password = protector.Protect(section.Password);
        }

        // Release the tenant to apply the settings.
        await _shellHost.ReleaseShellContextAsync(_shellSettings);

        return await EditAsync(section, context);
    }
}
