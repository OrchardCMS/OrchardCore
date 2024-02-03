using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class EmailSettingsDisplayDriver : SectionDisplayDriver<ISite, EmailSettings>
    {
        public const string GroupId = "email";
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public EmailSettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(EmailSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            var shapes = new List<IDisplayResult>
            {
                Initialize<EmailSettings>("EmailSettings_Edit", model =>
                {
                    model.DefaultSender = settings.DefaultSender;
                }).Location("Content:5").OnGroup(GroupId),
            };

            return Combine(shapes);
        }

        public override async Task<IDisplayResult> UpdateAsync(EmailSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            if (!context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            await context.Updater.TryUpdateModelAsync(section, Prefix);

            // Release the tenant to apply the settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            return await EditAsync(section, context);
        }
    }
}
